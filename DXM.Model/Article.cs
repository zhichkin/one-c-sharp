using System;
using System.Collections.Generic;
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

        private List<ArticleFilter> _filters = new List<ArticleFilter>();
        public List<ArticleFilter> Filters
        {
            get
            {
                if (this.state == PersistentState.New) return _filters;
                if (_filters.Count > 0) return _filters;
                _filters = ArticleFilter.Select(this);
                return _filters;
            }
        }
    }
}
