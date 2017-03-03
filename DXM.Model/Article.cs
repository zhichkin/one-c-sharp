using System;
using Zhichkin.ORM;
using Zhichkin.Metadata.Model;

namespace Zhichkin.DXM.Model
{
    public sealed partial class Article : EntityBase
    {
        private static readonly IDataMapper _mapper = DXMContext.Current.GetDataMapper(typeof(Article));

        private Publication _Publication = null;
        private Entity _Entity = null;

        public Article() : base(_mapper) { }
        public Article(Guid identity) : base(_mapper, identity) { }
        public Article(Guid identity, PersistentState state) : base(_mapper, identity, state) { }
        
        public Publication Publication { set { Set<Publication>(value, ref _Publication); } get { return Get<Publication>(ref _Publication); } }
        public Entity Entity { set { Set<Entity>(value, ref _Entity); } get { return Get<Entity>(ref _Entity); } }
    }
}
