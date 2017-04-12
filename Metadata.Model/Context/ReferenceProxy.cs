using System;
using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public sealed partial class ReferenceProxy : ReferenceObject
    {
        private readonly Entity _entity;
        private string _presentation = string.Empty;

        public ReferenceProxy(Entity entity, Guid identity) : base(null, identity, PersistentState.Virtual)
        {
            _entity = entity;
            this.mapper = new ReferenceProxy.DataMapper(_entity);
        }
        public Entity Type { get { return _entity; } }
        public override string ToString()
        {
            if (this.identity == Guid.Empty) return string.Empty;
            return Get<string>(ref _presentation);
        }
    }
}