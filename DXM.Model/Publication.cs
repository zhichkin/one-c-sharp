using System;
using Zhichkin.ORM;
using Zhichkin.Metadata.Model;

namespace Zhichkin.DXM.Model
{
    public sealed partial class Publication : EntityBase
    {
        private static readonly IDataMapper _mapper = DXMContext.Current.GetDataMapper(typeof(Publication));

        private InfoBase _InfoBase = null;

        public Publication() : base(_mapper) { }
        public Publication(Guid identity) : base(_mapper, identity) { }
        public Publication(Guid identity, PersistentState state) : base(_mapper, identity, state) { }
        
        public InfoBase InfoBase { set { Set<InfoBase>(value, ref _InfoBase); } get { return Get<InfoBase>(ref _InfoBase); } }
    }
}
