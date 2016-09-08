using System;
using System.Collections.Generic;
using Zhichkin.ORM;
using Zhichkin.ChangeTracking;
using Zhichkin.Integrator.Model;
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
        private readonly ChangeTrackingService ChangeTracking;
        public IntegratorService()
        {
            Factory = IntegratorPersistentContext.Current.Factory;
            ConnectionString = IntegratorPersistentContext.Current.ConnectionString;
            ChangeTracking = new ChangeTrackingService(ConnectionString);
        }

        public IList<Publisher> GetPublishers()
        {
            return Publisher.Select();
        }

        public void PublishChanges(Publisher publisher)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            
            List<ChangeTrackingRecord> changes = new List<ChangeTrackingRecord>();
            long current_sync_version = GetChangesToPublish(publisher, ref changes);
            if (current_sync_version == 0) return; // there is no new changes available

            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, options))
            {
                // save current synchronization version to be used the next time
                publisher.LastSyncVersion = current_sync_version;
                publisher.Save();

                // send changes to the target queue
                SendChanges(publisher, changes);

                scope.Complete();
            }
        }
        private long GetChangesToPublish(Publisher publisher, ref List<ChangeTrackingRecord> changes)
        {
            long current_sync_version = 0;

            //TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.Snapshot };
            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                current_sync_version = ValidateLastSyncVersion(publisher, command);

                if (current_sync_version != 0)
                {
                    changes = ChangeTracking.SelectChanges(publisher.Owner.MainTable, publisher.LastSyncVersion, command);
                }

                scope.Complete();
            }
            return current_sync_version;
        }
        private long ValidateLastSyncVersion(Publisher publisher, SqlCommand command)
        {
            long min_valid_version = ChangeTracking.GetMinValidSyncVersion(publisher.Owner.MainTable, command);
            long current_sync_version = ChangeTracking.GetCurrentSyncVersion(command);

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
            return new MessageQueue(GetQueuePath(publisher));
        }
        private void SendChanges(Publisher publisher, List<ChangeTrackingRecord> changes)
        {
            var serializer = new Serializer(new List<Type>() { typeof(ChangeTrackingRecord) });
            MessageQueue queue = GetPublisherQueue(publisher);
            foreach (ChangeTrackingRecord item in changes)
            {
                Message message = new Message();
                serializer.Serialize(message.BodyStream, item);
                message.Formatter = new BinaryMessageFormatter();
                queue.Send(message, MessageQueueTransactionType.Automatic);
            }
            queue.Close();
        }
        #endregion

        public void ProcessMessages(Publisher publisher) // operate by Subscription - process messages
        {
            MessageQueue queue = GetPublisherQueue(publisher);
            queue.Formatter = new BinaryMessageFormatter();
            var serializer = new Serializer(new List<Type>() { typeof(ChangeTrackingRecord) });
            
            bool queue_is_empty = false;
            while (!queue_is_empty)
            {
                try
                {
                    Message message = queue.Peek(new TimeSpan(0, 0, 0));
                    ChangeTrackingRecord change = (ChangeTrackingRecord)serializer.Deserialize(message.BodyStream);
                    queue.Receive();
                }
                catch (MessageQueueException msx)
                {
                    if (msx.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout) throw msx;
                    queue_is_empty = true;
                }
            }
        }
        //private void WriteChange(MessageTranslator translator, ChangeTrackingRecord record, SqlCommand command)
        //{
        //    command.Parameters.Clear();
        //    translator.Translate(record, command);
        //    command.ExecuteNonQuery();
        //}
    }
}
