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
        
        public Publisher() : base(_mapper) { throw new NotSupportedException(); }
        public Publisher(Guid identity) : this(identity, PersistentState.New) { }
        public Publisher(Guid identity, PersistentState state) : base(_mapper, identity, state)
        {
            owner = MetadataPersistentContext.Current.Factory.New<Entity>(identity);
        }
        private readonly Entity owner;
        ///<summary>Inheritance: one-to-one entity reference</summary>
        public Entity Owner { get { return owner; } }

        private long last_sync_version = 0;
        ///<summary>The last version number that was used to synchronize data</summary>
        public long LastSyncVersion { set { Set<long>(value, ref last_sync_version); } get { return Get<long>(ref last_sync_version); } }

        private string msmq_target_queue = string.Empty;
        ///<summary>The target MSMQ queue to send messages with data changes to</summary>
        public string MSMQTargetQueue { set { Set<string>(value, ref msmq_target_queue); } get { return Get<string>(ref msmq_target_queue); } }
    }
}
