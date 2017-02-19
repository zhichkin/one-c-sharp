using System;
using System.IO;
using System.Xml;
using System.Text;
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

        //+++ XML version
        //public int PublishChanges(Publisher publisher)
        //{
        //    if (publisher == null) throw new ArgumentNullException("publisher");
        //    if (publisher.ChangeTrackingSystem == ChangeTrackingSystem.None) return 0;

        //    string connectionString = publisher.Entity.InfoBase.ConnectionString;
        //    ChangeTrackingService service = new ChangeTrackingService(connectionString);
        //    ChangeTrackingTableInfo info = service.GetChangeTrackingTableInfo(publisher.Entity.MainTable);
        //    if (info == null) return 0;

        //    int messagesSent = 0;
        //    List<ChangeTrackingMessage> changes = new List<ChangeTrackingMessage>();
        //    long current_sync_version = GetChangesToPublish(publisher, ref changes);
        //    if (current_sync_version == 0) return 0; // there is no new changes available

        //    TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
        //    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, options))
        //    {
        //        // save current synchronization version to be used the next time
        //        publisher.LastSyncVersion = current_sync_version;
        //        publisher.Save();

        //        // send changes to the target queue
        //        messagesSent = SendChanges(publisher, changes);

        //        scope.Complete();
        //    }
        //    return messagesSent;
        //}
        public int PublishChanges(Publisher publisher)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (publisher.ChangeTrackingSystem == ChangeTrackingSystem.None) return 0;

            string connectionString = publisher.Entity.InfoBase.ConnectionString;
            ChangeTrackingService service = new ChangeTrackingService(connectionString);

            // check if change tracking is enabled
            ChangeTrackingTableInfo info = service.GetChangeTrackingTableInfo(publisher.Entity.MainTable);
            if (info == null) return 0;

            int messagesSent = 0;
            //TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.Snapshot };
            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                long current_sync_version = ValidateLastSyncVersion(publisher, command);
                Dictionary<InfoBase, MessageQueue> queues = null;
                if (current_sync_version != 0)
                {
                    //MessageQueue queue = GetPublisherQueue(publisher);
                    IMessageFormatter formatter = new BinaryMessageFormatter();
                    queues = GetMessageQueues(publisher);
                    Dictionary<Subscription, XMLMessageProducer> producers = GetMessageProducers(publisher);
                    service.PrepareSelectChangesCommand(publisher.Entity.MainTable, publisher.LastSyncVersion, command);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            foreach (Subscription subscription in publisher.Subscriptions)
                            {
                                Message message = new Message() { Formatter = formatter };
                                producers[subscription].Produce(reader, message.BodyStream);
                                // send changes to the target queue
                                queues[subscription.Subscriber.InfoBase].Send(message, MessageQueueTransactionType.Automatic);
                                messagesSent++;
                            }
                        }
                    }
                    // save current synchronization version to be used the next time
                    publisher.LastSyncVersion = current_sync_version;
                    publisher.Save();
                }
                scope.Complete();
                if (queues != null) CloseQueues(queues);
            }
            return messagesSent;
        }
        private Dictionary<InfoBase, MessageQueue> GetMessageQueues(Publisher publisher)
        {
            Dictionary<InfoBase, MessageQueue> queues = new Dictionary<InfoBase, MessageQueue>();
            foreach (Subscription subscription in publisher.Subscriptions)
            {
                InfoBase infoBase = subscription.Subscriber.InfoBase;
                if (queues.ContainsKey(infoBase)) continue;
                MessageQueue queue = GetMessageQueue(infoBase);
                queues.Add(infoBase, queue);
            }
            return queues;
        }
        private void CloseQueues(Dictionary<InfoBase, MessageQueue> queues)
        {
            foreach (MessageQueue queue in queues.Values)
            {
                queue.Close();
            }
        }
        private Dictionary<Subscription, XMLMessageProducer> GetMessageProducers(Publisher publisher)
        {
            Dictionary<Subscription, XMLMessageProducer> producers = new Dictionary<Subscription, XMLMessageProducer>();
            foreach (Subscription subscription in publisher.Subscriptions)
            {
                producers.Add(subscription, new XMLMessageProducer().Use(subscription));
            }
            return producers;
        }
        //---
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
        private string GetQueuePath(InfoBase infoBase)
        {
            return CONST_MessageQueuePrefix + infoBase.Identity.ToString();
        }
        private MessageQueue GetMessageQueue(InfoBase infoBase)
        {
            string path = GetQueuePath(infoBase);
            if (MessageQueue.Exists(path))
            {
                return new MessageQueue(path);
            }
            // create transactional message queue
            MessageQueue queue = MessageQueue.Create(path, true);
            string label = infoBase.ToString();
            queue.Label = (label.Length > 124) ? label.Substring(0, 124) : label;
            return queue;
        }
        public void DeleteQueue(InfoBase infoBase)
        {
            string path = GetQueuePath(infoBase);
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
            //Message message = queue.Receive(TimeSpan.FromSeconds(1));
            //return ReadXMLStream(message.BodyStream);
        }
        private string ReadXMLStream(Stream stream)
        {
            StringBuilder sb = new StringBuilder();
            using (XmlReader reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        sb.AppendLine(reader.Name);
                        if (reader.HasAttributes)
                        {
                            while (reader.MoveToNextAttribute())
                            {
                                sb.AppendLine(string.Format("{0}=\"{1}\"", reader.Name, reader.Value));
                            }
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.Text) { sb.AppendLine(reader.Value); }
                    else if (reader.NodeType == XmlNodeType.EndElement) { sb.AppendLine(reader.Name); }
                }
            }
            return sb.ToString();
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

        private Dictionary<Guid, IMessageTranslator<ChangeTrackingMessage>> translators = new Dictionary<Guid, IMessageTranslator<ChangeTrackingMessage>>();
        private Dictionary<string, Action<Subscription, ChangeTrackingMessage, SqlCommand>> processingStrategies = new Dictionary<string, Action<Subscription, ChangeTrackingMessage, SqlCommand>>();
        private IMessageTranslator<ChangeTrackingMessage> GetMessageTranslator(Subscription subscription)
        {
            IMessageTranslator<ChangeTrackingMessage> translator;
            if (translators.TryGetValue(subscription.Identity, out translator))
            {
                return translator;
            }
            translator = new MessageTranslator(subscription);
            translators.Add(subscription.Identity, translator);
            return translator;
        }
        //+++ XML version
        //public int ProcessMessages(Publisher publisher)
        //{
        //    int messagesProcessed = 0;
        //    IList<Subscription> subscriptions = publisher.Subscriptions;
        //    if (subscriptions == null || subscriptions.Count == 0) return messagesProcessed;

        //    MessageQueue queue = GetPublisherQueue(publisher);
        //    queue.Formatter = new BinaryMessageFormatter();
        //    Serializer serializer = new Serializer(new List<Type>() { typeof(ChangeTrackingMessage) });
            
        //    bool queue_is_empty = false;
        //    while (!queue_is_empty)
        //    {
        //        try
        //        {
        //            Message message = queue.Peek(new TimeSpan(0, 0, 0));
        //            ChangeTrackingMessage change = (ChangeTrackingMessage)serializer.Deserialize(message.BodyStream);
        //            foreach (Subscription subscription in subscriptions)
        //            {
        //                ProcessMessage(subscription, change);
        //            }
        //            queue.Receive();
        //            messagesProcessed++;
        //        }
        //        catch (MessageQueueException msx)
        //        {
        //            if (msx.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout) throw msx;
        //            queue_is_empty = true;
        //        }
        //    }
        //    return messagesProcessed;
        //}
        public int ProcessMessages(Publisher publisher)
        {
            int messagesProcessed = 0;
            IList<Subscription> subscriptions = publisher.Subscriptions;
            if (subscriptions == null || subscriptions.Count == 0) return messagesProcessed;

            Dictionary<InfoBase, MessageQueue> queues = GetMessageQueues(publisher);
            IMessageFormatter formatter = new BinaryMessageFormatter();
            foreach (KeyValuePair<InfoBase, MessageQueue> item in queues)
            {
                InfoBase infoBase = item.Key;
                MessageQueue queue = item.Value;
                queue.Formatter = new BinaryMessageFormatter();

                using (SqlConnection connection = new SqlConnection(infoBase.ConnectionString))
                {
                    connection.Open();
                    bool queue_is_empty = false;
                    while (!queue_is_empty)
                    {
                        try
                        {
                            Message message = queue.Peek(new TimeSpan(0, 0, 0));
                            IDataMessage data = new XMLMessageConsumer().Consume(message.BodyStream);
                            ProcessDataMessage(connection, data);
                            queue.Receive();
                            messagesProcessed++;
                        }
                        catch (MessageQueueException msx)
                        {
                            if (msx.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout) throw msx;
                            queue_is_empty = true;
                        }
                    }
                }
            }
            return messagesProcessed;
        }
        private void ProcessDataMessage(IDbConnection connection, IDataMessage message)
        {
            //TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            //using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            using(IDbTransaction transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
            using (IDbCommand command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                Subscription subscription = Subscription.Select(new Guid(message.Key));
                foreach (IDataEntity entity in message.Entities)
                {
                    ProcessDataEntity(subscription, entity, command);
                }
                transaction.Commit();
            }
        }
        private void ProcessDataEntity(Subscription subscription, IDataEntity entity, IDbCommand command)
        {
            InfoBase source = subscription.Publisher.Entity.InfoBase;
            Entity metadata = Entity.Select(new Guid(entity.Key));
            ICommandBuilder builder = new DataCommandBuilder(metadata) { ChangeTrackingContext = source };
            foreach (DataRecord record in entity.Records)
            {
                if (record.RecordType == DataRecordType.Insert) ExecuteInsertCommand(subscription, builder, command, record);
                else if (record.RecordType == DataRecordType.Update) ExecuteUpdateCommand(subscription, builder, command, record);
                else if (record.RecordType == DataRecordType.Delete) ExecuteDeleteCommand(subscription, builder, command, record);
            }
        }
        private void ExecuteInsertCommand(Subscription subscription, ICommandBuilder builder, IDbCommand command, DataRecord record)
        {
            builder.PrepareInsert(command, record);
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
                record.RecordType = DataRecordType.Update;
                ExecuteUpdateCommand(subscription, builder, command, record);
            }
        }
        private void ExecuteUpdateCommand(Subscription subscription, ICommandBuilder builder, IDbCommand command, DataRecord record)
        {
            builder.PrepareUpdate(command, record);
            int rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected > 0) return;

            // update-delete or update-none collision
            if (subscription.OnUpdateCollision == CollisionResolutionStrategy.Ignore) return;
            if (subscription.OnUpdateCollision == CollisionResolutionStrategy.Insert)
            {
                record.RecordType = DataRecordType.Insert;
                ExecuteInsertCommand(subscription, builder, command, record);
            }
            //TODO: CollisionResolutionStrategy.RaiseError
        }
        private void ExecuteDeleteCommand(Subscription subscription, ICommandBuilder builder, IDbCommand command, DataRecord record)
        {
            builder.PrepareDelete(command, record);
            int rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected > 0) return;

            // delete-delete or delete-none collision
            if (subscription.OnDeleteCollision == CollisionResolutionStrategy.Ignore) return;
            //TODO: CollisionResolutionStrategy.RaiseError
        }
        //---
        private void ProcessMessage(Subscription subscription, ChangeTrackingMessage message)
        {
            ChangeTrackingMessage target = GetMessageTranslator(subscription).Translate(message);

            string connectionString = subscription.Subscriber.InfoBase.ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                processingStrategies[target.SYS_CHANGE_OPERATION](subscription, target, command);
            }
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
            int rowsAffected = 0;
            rowsAffected = command.ExecuteNonQuery();
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

        public Dictionary<int, int> GetTypeCodesLookup(Subscription subscription)
        {
            Dictionary<int, int> lookup = new Dictionary<int, int>();
            using (SqlConnection connection = new SqlConnection(IntegratorPersistentContext.Current.ConnectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "[integrator].[get_type_codes_lookup]";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("source_infobase", subscription.Publisher.Entity.InfoBase.Identity);
                    command.Parameters.AddWithValue("target_infobase", subscription.Subscriber.InfoBase.Identity);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lookup.Add(
                                reader.GetInt32(0),
                                reader.GetInt32(1));
                        }
                    }
                }
            }
            return lookup;
        }

        public void ExecuteNewScopeCommand(InfoBase infoBase, ICommandExecutor executor)
        {
            string connectionString = infoBase.ConnectionString;
            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, options))
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                executor.Execute(command);
                scope.Complete();
            }
        }
    }
}
