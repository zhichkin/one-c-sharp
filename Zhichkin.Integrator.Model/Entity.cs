using System;
using Zhichkin.ORM;
using System.Collections.Generic;
using M = Zhichkin.Metadata.Model;

namespace Zhichkin.Integrator.Model
{
    public sealed partial class Entity : M.EntityBase
    {
        private static readonly IDataMapper _mapper = IntegratorPersistentContext.Current.GetDataMapper(typeof(Entity));
        public static IList<Entity> Select() { return DataMapper.Select(); }
        public static Entity Select(Guid identity) { return DataMapper.Select(identity); }
        
        public Entity() : base(_mapper) { throw new NotSupportedException(); }
        public Entity(Guid identity) : this(identity, PersistentState.New) { }
        public Entity(Guid identity, PersistentState state) : base(_mapper, identity, state)
        {
            parent = M.MetadataPersistentContext.Current.Factory.New<M.Entity>(identity);
        }
        private readonly M.Entity parent;
        ///<summary>Inheritance: one-to-one entity reference</summary>
        public M.Entity Parent { get { return parent; } }

        private long last_sync_version = 0;
        ///<summary>The last version number that was used to synchronize data</summary>
        public long LastSyncVersion { set { Set<long>(value, ref last_sync_version); } get { return Get<long>(ref last_sync_version); } }

        private string msmq_target_queue = string.Empty;
        ///<summary>The target MSMQ queue to send messages with data changes to</summary>
        public string MSMQTargetQueue { set { Set<string>(value, ref msmq_target_queue); } get { return Get<string>(ref msmq_target_queue); } }
    }
}
