using System;
using System.Collections.Generic;
using Zhichkin.ChangeTracking;
using Zhichkin.Integrator.Model;
using System.Messaging;
using NetSerializer;

namespace Zhichkin.Integrator.Services
{
    public sealed class IntegratorService : IIntegratorService
    {
        public IList<Entity> GetPublishers()
        {
            return Entity.Select();
        }
        public void PublishChanges(Entity publisher)
        {

        }
        #region " MSMQ "
        private const string CONST_MessageQueuePrefix = @".\PRIVATE$\";
        private string GetQueuePath(Entity publisher)
        {
            return CONST_MessageQueuePrefix + publisher.Identity.ToString();
        }
        private void CreateQueue(Entity publisher)
        {
            string path = GetQueuePath(publisher);
            if (!MessageQueue.Exists(path))
            {
                // create transactional message queue
                MessageQueue.Create(path, true); 
            }
        }
        private void DeleteQueue(Entity publisher)
        {
            string path = GetQueuePath(publisher);
            if (MessageQueue.Exists(path))
            {
                //TODO: check if this queue referenced by several subscribers
                MessageQueue.Delete(path);
            }
        }
        private MessageQueue GetPublisherQueue(Entity publisher)
        {
            return new MessageQueue(GetQueuePath(publisher));
        }
        private void SendChanges(Entity publisher, List<ChangeTrackingRecord> changes)
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
        //public void ExportChanges(Subscription subscription)
        //{
        //    if (subscription == null) throw new ArgumentNullException("subscription");

        //    long min_valid_version = 0;
        //    long current_sync_version = 0;
        //    List<ChangeTrackingRecord> changes = null;
        //    string connectionString = Model.Services.GetConnectionString(subscription.Source);

        //    TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.Snapshot };
        //    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
        //    using (SqlConnection connection = new SqlConnection(connectionString))
        //    using (SqlCommand command = connection.CreateCommand())
        //    {
        //        connection.Open();

        //        // validate last synchronization export version
        //        min_valid_version = GetMinValidSyncVersion(subscription.Source, command);
        //        if (subscription.ExportSyncVersion < min_valid_version)
        //        {
        //            current_sync_version = GetCurrentSyncVersion(command);
        //            throw new Exception("Our synchronization version is too old - we must reinitialize!"
        //                + Environment.NewLine +
        //                "Current synchronization version is " + current_sync_version.ToString());
        //        }

        //        // get current synchronization version to be used the next time
        //        current_sync_version = GetCurrentSyncVersion(command);
        //        if (subscription.ExportSyncVersion > current_sync_version)
        //        { throw new Exception("Source has older data (might be restored from backup)!"); }

        //        if (subscription.ExportSyncVersion == current_sync_version)
        //        { throw new ApplicationException("There is nothing to export!"); }

        //        // get changes to export
        //        changes = ReadChanges(subscription, command);

        //        if (changes.Count == 0)
        //        { throw new ApplicationException("There is no changes to export!"); }

        //        scope.Complete();
        //    }

        //    options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
        //    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, options))
        //    {
        //        // save current synchronization version
        //        //subscription.Load(); // TODO: issue key lock
        //        subscription.ExportSyncVersion = current_sync_version;
        //        subscription.Save();

        //        // send changes to the target queue
        //        SendChanges(changes, subscription);

        //        scope.Complete();
        //    }
        //}
    }
}
