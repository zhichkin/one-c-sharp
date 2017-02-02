using System;
using Zhichkin.ORM;
using System.Collections.Generic;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Integrator.Model
{
    public sealed partial class Publisher : EntityBase
    {
        private static readonly IDataMapper _mapper = IntegratorPersistentContext.Current.GetDataMapper(typeof(Publisher));
        public static IList<Publisher> Select() { return DataMapper.Select(); }
        public static Publisher Select(Guid identity) { return DataMapper.Select(identity); }
        public static Publisher SelectOrCreate(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            Publisher publisher = Publisher.Select(entity.Identity);
            if (publisher == null)
            {
                publisher = (Publisher)IntegratorPersistentContext.Current.Factory.New(typeof(Publisher), entity.Identity);
                publisher.Name = entity.FullName;
                publisher.LastSyncVersion = 0;
                publisher.Save();
            }
            return publisher;
        }

        public Publisher() : base(_mapper) { throw new NotSupportedException(); }
        public Publisher(Guid identity) : this(identity, PersistentState.New) { }
        public Publisher(Guid identity, PersistentState state) : base(_mapper, identity, state)
        {
            entity = MetadataPersistentContext.Current.Factory.New<Entity>(identity);
        }

        private readonly Entity entity;
        private long last_sync_version = 0;
        private string msmq_target_queue = string.Empty;
        private ChangeTrackingSystem change_tracking_system = ChangeTrackingSystem.None;

        ///<summary>Inheritance: one-to-one entity reference</summary>
        public Entity Entity { get { return entity; } }
        ///<summary>The last version number that was used to synchronize data</summary>
        public long LastSyncVersion { set { Set<long>(value, ref last_sync_version); } get { return Get<long>(ref last_sync_version); } }
        ///<summary>The target MSMQ queue to send messages with data changes to</summary>
        public string MSMQTargetQueue { set { Set<string>(value, ref msmq_target_queue); } get { return Get<string>(ref msmq_target_queue); } }
        ///<summary>Change tracking system used to track data changes of entities</summary>
        public ChangeTrackingSystem ChangeTrackingSystem { set { Set<ChangeTrackingSystem>(value, ref change_tracking_system); } get { return Get<ChangeTrackingSystem>(ref change_tracking_system); } }

        private IList<Subscription> subscriptions = new List<Subscription>();
        public IList<Subscription> Subscriptions
        {
            get
            {
                if (this.state == PersistentState.New) return subscriptions;
                if (subscriptions.Count > 0) return subscriptions;
                subscriptions = Subscription.Select(this);
                return subscriptions;
            }
        }
    }
}
