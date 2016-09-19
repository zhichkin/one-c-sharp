using System;
using System.Collections.Generic;
using Zhichkin.ORM;
using Zhichkin.ChangeTracking;
using Zhichkin.Metadata.Model;
using Zhichkin.Integrator.Model;
using Zhichkin.Integrator.Translator;
using System.Messaging;
using NetSerializer;
using System.Transactions;
using System.Data;
using System.Data.SqlClient;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace Zhichkin.Integrator.Services
{
    public sealed class IntegratorService : IIntegratorService
    {
        public string ConnectionString { get; private set; }
        public IReferenceObjectFactory Factory { get; private set; }
        public IntegratorService()
        {
            Factory = IntegratorPersistentContext.Current.Factory;
            ConnectionString = IntegratorPersistentContext.Current.ConnectionString;
        }
        public IList<Publisher> GetPublishers()
        {
            return Publisher.Select();
        }
        public int PublishChanges(Publisher publisher)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");

            int messagesSent = 0;
            List<ChangeTrackingRecord> changes = new List<ChangeTrackingRecord>();
            long current_sync_version = GetChangesToPublish(publisher, ref changes);
            if (current_sync_version == 0) return 0; // there is no new changes available

            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, options))
            {
                // save current synchronization version to be used the next time
                publisher.LastSyncVersion = current_sync_version;
                publisher.Save();

                // send changes to the target queue
                messagesSent = SendChanges(publisher, changes);

                scope.Complete();
            }
            return messagesSent;
        }
        private long GetChangesToPublish(Publisher publisher, ref List<ChangeTrackingRecord> changes)
        {
            string connectionString = publisher.Entity.InfoBase.ConnectionString;
            ChangeTrackingService service = new ChangeTrackingService(connectionString);

            long current_sync_version = 0;

            //TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.Snapshot };
            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                current_sync_version = ValidateLastSyncVersion(publisher, command);

                if (current_sync_version != 0)
                {
                    changes = service.SelectChanges(publisher.Entity.MainTable, publisher.LastSyncVersion, command);
                }

                scope.Complete();
            }
            return current_sync_version;
        }
        private long ValidateLastSyncVersion(Publisher publisher, SqlCommand command)
        {
            ChangeTrackingService service = new ChangeTrackingService(publisher.Entity.InfoBase.ConnectionString);

            long min_valid_version = service.GetMinValidSyncVersion(publisher.Entity.MainTable, command);
            long current_sync_version = service.GetCurrentSyncVersion(command);

            // this is the first synchronization time 
            if (publisher.LastSyncVersion == 0) return current_sync_version;

            // validate last synchronization version
            if (publisher.LastSyncVersion < min_valid_version)
            {
                throw new Exception("Our synchronization version is too old - reinitialize!"
                    + Environment.NewLine +
                    "The last synchronization version: " + publisher.LastSyncVersion.ToString()
                    + Environment.NewLine +
                    "Minimum valid synchronization version: " + min_valid_version.ToString());
            }

            if (publisher.LastSyncVersion > current_sync_version)
            { throw new Exception("Source has older data (might be restored from backup)!"); }

            // there is no new changes available
            if (publisher.LastSyncVersion == current_sync_version) return 0;

            return current_sync_version;
        }

        #region " MSMQ "
        private const string CONST_MessageQueuePrefix = @".\PRIVATE$\";
        private string GetQueuePath(Publisher publisher)
        {
            return CONST_MessageQueuePrefix + publisher.Identity.ToString();
        }
        private void CreateQueue(Publisher publisher)
        {
            string path = GetQueuePath(publisher);
            if (!MessageQueue.Exists(path))
            {
                // create transactional message queue
                MessageQueue.Create(path, true); 
            }
        }
        private void DeleteQueue(Publisher publisher)
        {
            string path = GetQueuePath(publisher);
            if (MessageQueue.Exists(path))
            {
                //TODO: check if this queue referenced by several subscribers
                MessageQueue.Delete(path);
            }
        }
        private MessageQueue GetPublisherQueue(Publisher publisher)
        {
            if (string.IsNullOrWhiteSpace(publisher.MSMQTargetQueue))
            {
                publisher.MSMQTargetQueue = GetQueuePath(publisher);
                publisher.Save();
                CreateQueue(publisher);
            }
            return new MessageQueue(publisher.MSMQTargetQueue);
        }
        private int SendChanges(Publisher publisher, List<ChangeTrackingRecord> changes)
        {
            int messagesSent = 0;
            var serializer = new Serializer(new List<Type>() { typeof(ChangeTrackingRecord) });
            MessageQueue queue = GetPublisherQueue(publisher);
            foreach (ChangeTrackingRecord item in changes)
            {
                Message message = new Message();
                serializer.Serialize(message.BodyStream, item);
                message.Formatter = new BinaryMessageFormatter();
                queue.Send(message, MessageQueueTransactionType.Automatic);
                messagesSent++;
            }
            queue.Close();
            return messagesSent;
        }
        #endregion

        public IList<Subscription> GetSubscriptions()
        {
            return Subscription.Select();
        }
        public int ProcessMessages(Subscription subscription)
        {
            string connectionString = subscription.Subscriber.InfoBase.ConnectionString;
            IMessageTranslator<ChangeTrackingRecord> translator = new MessageTranslator(subscription);
            ChangeTrackingRecordToSqlCommandAdapter adapter = new ChangeTrackingRecordToSqlCommandAdapter();

            MessageQueue source = GetPublisherQueue(subscription.Publisher);
            source.Formatter = new BinaryMessageFormatter();
            var serializer = new Serializer(new List<Type>() { typeof(ChangeTrackingRecord) });

            int messagesProcessed = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandType = CommandType.Text;
                bool queue_is_empty = false;
                while (!queue_is_empty)
                {
                    try
                    {
                        Message message = source.Peek(new TimeSpan(0, 0, 0));
                        ChangeTrackingRecord change = (ChangeTrackingRecord)serializer.Deserialize(message.BodyStream);
                        ChangeTrackingRecord target = translator.Translate(change);
                        adapter.Use(subscription).Input(target).Output(command);
                        command.ExecuteNonQuery();
                        source.Receive();
                        messagesProcessed++;
                    }
                    catch (MessageQueueException msx)
                    {
                        if (msx.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout) throw msx;
                        queue_is_empty = true;
                    }
                }
            }
            return messagesProcessed;
        }

        public Subscription CreateSubscription(Publisher publisher, Entity subscriber)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (subscriber == null) throw new ArgumentNullException("subscriber");
            if (publisher == subscriber) throw new InvalidOperationException("Publisher and subscriber can not be the same!");

            Subscription subscription = (Subscription)Factory.New(typeof(Subscription));
            subscription.Publisher = publisher;
            subscription.Subscriber = subscriber;
            subscription.Name = subscription.ToString();
            subscription.Save();
            return subscription;
        }
        public void DeleteSubscription(Subscription subscription)
        {
            if (subscription == null) throw new ArgumentNullException("subscription");
            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, options))
            {
                foreach (TranslationRule rule in subscription.TranslationRules)
                {
                    rule.Kill();
                }
                subscription.Kill();
                scope.Complete();
            }
        }
    }
}
