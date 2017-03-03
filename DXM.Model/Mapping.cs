using System;
using Zhichkin.ORM;
using Zhichkin.Metadata.Model;

namespace Zhichkin.DXM.Model
{
    public sealed partial class Mapping : EntityBase
    {
        private static readonly IDataMapper _mapper = DXMContext.Current.GetDataMapper(typeof(Mapping));

        private Property _Source = null;
        private Property _Target = null;
        private bool _IsSyncKey = false;

        public Mapping() : base(_mapper) { }
        public Mapping(Guid identity) : base(_mapper, identity) { }
        public Mapping(Guid identity, PersistentState state) : base(_mapper, identity, state) { }
        
        public Property Source { set { Set<Property>(value, ref _Source); } get { return Get<Property>(ref _Source); } }
        public Property Target { set { Set<Property>(value, ref _Target); } get { return Get<Property>(ref _Target); } }
        public bool IsSyncKey { set { Set<bool>(value, ref _IsSyncKey); } get { return Get<bool>(ref _IsSyncKey); } }
    }
}
