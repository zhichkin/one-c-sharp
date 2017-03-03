using System;
using Zhichkin.ORM;
using Zhichkin.Metadata.Model;

namespace Zhichkin.DXM.Model
{
    public sealed partial class Subscription : EntityBase
    {
        private static readonly IDataMapper _mapper = DXMContext.Current.GetDataMapper(typeof(Subscription));

        private Entity _Source = null;
        private Entity _Target = null;

        public Subscription() : base(_mapper) { }
        public Subscription(Guid identity) : base(_mapper, identity) { }
        public Subscription(Guid identity, PersistentState state) : base(_mapper, identity, state) { }
        
        public Entity Source { set { Set<Entity>(value, ref _Source); } get { return Get<Entity>(ref _Source); } }
        public Entity Target { set { Set<Entity>(value, ref _Target); } get { return Get<Entity>(ref _Target); } }
    }
}
