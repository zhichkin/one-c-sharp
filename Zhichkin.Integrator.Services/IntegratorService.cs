using System;
using System.Linq;
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
            processingStrategies.Add("I", this.ProcessInsertMessage);
            processingStrategies.Add("U", this.ProcessUpdateMessage);
            processingStrategies.Add("D", this.ProcessDeleteMessage);
        }
        public IList<Publisher> GetPublishers()
        {
            return Publisher.Select();
        }

        public int CountChanges(Publisher publisher)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            int count = 0;
            string connectionString = publisher.Entity.InfoBase.ConnectionString;
            ChangeTrackingService service = new ChangeTrackingService(connectionString);
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                count = service.CountChanges(publisher.Entity.MainTable, publisher.LastSyncVersion, command);
            }
            return count;
        }

        public int PublishChanges(Publisher publisher)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");

            string connectionString = publisher.Entity.InfoBase.ConnectionString;
            ChangeTrackingService service = new ChangeTrackingService(connectionString);
            ChangeTrackingTableInfo info = service.GetChangeTrackingTableInfo(publisher.Entity.MainTable);
            if (info == null) return 0;

            int messagesSent = 0;
            List<ChangeTrackingMessage> changes = new List<ChangeTrackingMessage>();
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
        private long GetChangesToPublish(Publisher publisher, ref List<ChangeTrackingMessage> changes)
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
        private int SendChanges(Publisher publisher, List<ChangeTrackingMessage> changes)
        {
            int messagesSent = 0;
            var serializer = new Serializer(new List<Type>() { typeof(ChangeTrackingMessage) });
            MessageQueue queue = GetPublisherQueue(publisher);
            foreach (ChangeTrackingMessage item in changes)
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
        
        #region " MSMQ "
        private const string CONST_MessageQueuePrefix = @".\PRIVATE$\";
        private string GetQueuePath(Publisher publisher)
        {
            return CONST_MessageQueuePrefix + publisher.Identity.ToString();
        }
        public void CreateQueue(Publisher publisher)
        {
            string path = GetQueuePath(publisher);
            if (!MessageQueue.Exists(path))
            {
                // create transactional message queue
                MessageQueue queue = MessageQueue.Create(path, true);
                string label = publisher.Entity.ToString();
                queue.Label = (label.Length > 124) ? label.Substring(0, 124) : label;
            }
        }
        public void DeleteQueue(Publisher publisher)
        {
            string path = GetQueuePath(publisher);
            if (MessageQueue.Exists(path))
            {
                //TODO: check if this queue referenced by several subscribers
                MessageQueue.Delete(path);
            }
        }
        public string TestQueue(Publisher publisher, string messageText)
        {
            MessageQueue queue = GetPublisherQueue(publisher);
            Message message = new Message(messageText);
            queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
            queue.Send(message, MessageQueueTransactionType.Single);
            message = queue.Receive(TimeSpan.FromSeconds(1));
            return (string)message.Body;
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
        public int GetPublishersQueuesLength()
        {
            int result = 0;
            foreach (Publisher publisher in GetPublishers())
            {
                string path = GetQueuePath(publisher);
                if (!MessageQueue.Exists(path)) continue;
                MessageQueue queue = new MessageQueue(path, false, true, QueueAccessMode.Peek);
                MessagePropertyFilter filter = new MessagePropertyFilter();
                filter.ClearAll();
                queue.MessageReadPropertyFilter = filter;
                result += queue.GetAllMessages().Length;
            }
            return result;
        }
        #endregion

        private Dictionary<string, Action<Subscription, ChangeTrackingMessage, SqlCommand>> processingStrategies = new Dictionary<string, Action<Subscription, ChangeTrackingMessage, SqlCommand>>();
        public IList<Subscription> GetSubscriptions()
        {
            return Subscription.Select();
        }
        public int ProcessMessages(Subscription subscription)
        {
            string connectionString = subscription.Subscriber.InfoBase.ConnectionString;
            IMessageTranslator<ChangeTrackingMessage> translator = new MessageTranslator(subscription);
            
            MessageQueue source = GetPublisherQueue(subscription.Publisher);
            source.Formatter = new BinaryMessageFormatter();
            var serializer = new Serializer(new List<Type>() { typeof(ChangeTrackingMessage) });

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
                        ChangeTrackingMessage change = (ChangeTrackingMessage)serializer.Deserialize(message.BodyStream);
                        ChangeTrackingMessage target = translator.Translate(change);
                        processingStrategies[target.SYS_CHANGE_OPERATION](subscription, target, command);
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
        private void ProcessInsertMessage(Subscription subscription, ChangeTrackingMessage message, SqlCommand command)
        {
            ChangeTrackingRecordToSqlCommandAdapter adapter = new ChangeTrackingRecordToSqlCommandAdapter();
            adapter.Use(subscription).Input(message).Output(command);
            int rowsAffected = 0;
            try
            {
                rowsAffected = command.ExecuteNonQuery();
            }
            catch
            {
                // insert-insert collision
                if (subscription.OnInsertCollision == CollisionResolutionStrategy.RaiseError) throw;
            }
            if (rowsAffected > 0) return;
            // insert-insert collision
            if (subscription.OnInsertCollision == CollisionResolutionStrategy.Ignore) return;
            if (subscription.OnInsertCollision == CollisionResolutionStrategy.Update)
            {
                message.SYS_CHANGE_OPERATION = "U";
                adapter.Use(subscription).Input(message).Output(command);
                rowsAffected = command.ExecuteNonQuery();
            }
        }
        private void ProcessUpdateMessage(Subscription subscription, ChangeTrackingMessage message, SqlCommand command)
        {
            ChangeTrackingRecordToSqlCommandAdapter adapter = new ChangeTrackingRecordToSqlCommandAdapter();
            adapter.Use(subscription).Input(message).Output(command);
            int rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected > 0) return;
            // update-delete or update-none collision
            if (subscription.OnUpdateCollision == CollisionResolutionStrategy.Ignore) return;
            if (subscription.OnUpdateCollision == CollisionResolutionStrategy.Insert)
            {
                message.SYS_CHANGE_OPERATION = "I";
                ProcessInsertMessage(subscription, message, command);
            }
            //TODO: CollisionResolutionStrategy.RaiseError
        }
        private void ProcessDeleteMessage(Subscription subscription, ChangeTrackingMessage message, SqlCommand command)
        {
            ChangeTrackingRecordToSqlCommandAdapter adapter = new ChangeTrackingRecordToSqlCommandAdapter();
            adapter.Use(subscription).Input(message).Output(command);
            int rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected > 0) return;
            // delete-delete or delete-none collision
            if (subscription.OnDeleteCollision == CollisionResolutionStrategy.Ignore) return;
            //TODO: CollisionResolutionStrategy.RaiseError
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
        public void DeletePublisher(Publisher publisher)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, options))
            {
                foreach (Subscription subscription in Subscription.Select(publisher))
                {
                    foreach (TranslationRule rule in subscription.TranslationRules)
                    {
                        rule.Kill();
                    }
                    subscription.Kill();
                }
                publisher.Kill();
                scope.Complete();
            }
        }
        public void CreateTranslationRules(Subscription subscription)
        {
            foreach (Property property in subscription.Publisher.Entity.Properties)
            {
                TranslationRule rule = subscription.TranslationRules
                    .Where(i => i.SourceProperty == property).FirstOrDefault();
                if (rule != null) continue;
                
                Property targetProperty = subscription.Subscriber.Properties
                    .Where(p => p.Name == property.Name).FirstOrDefault();
                if (targetProperty == null) continue;

                rule = subscription.CreateTranslationRule();
                rule.SourceProperty = property;
                rule.TargetProperty = targetProperty;
                rule.IsSyncKey = rule.TargetProperty.Fields.Where(f => f.IsPrimaryKey).FirstOrDefault() != null;
                rule.Save();
            }
        }
    }
}
