using System.Collections.Generic;
using Zhichkin.Metadata.Model;
using Zhichkin.ORM;

namespace Zhichkin.DXM.Model
{
    public sealed partial class ArticleFilter : ValueObject
    {
        private static readonly IDataMapper _mapper = DXMContext.Current.GetDataMapper(typeof(ArticleFilter));

        public static List<ArticleFilter> Select(Article article) { return DataMapper.Select(article); }

        private Article _article = null; private Article _article_old;
        private Property _property = null; private Property _property_old;
        private FilterOperator _operator = FilterOperator.Equal;
        private Entity _type = Entity.Empty;
        private object _value = null;

        public ArticleFilter() : base(_mapper) { }
        public ArticleFilter(PersistentState state) : base(_mapper, state) { }

        public Article Article { set { Set<Article>(value, ref _article); } get { return Get<Article>(ref _article); } }
        public Property Property { set { Set<Property>(value, ref _property); } get { return Get<Property>(ref _property); } }
        public FilterOperator Operator { set { Set<FilterOperator>(value, ref _operator); } get { return Get<FilterOperator>(ref _operator); } }
        public Entity Type
        {
            get { return Get<Entity>(ref _type); }
            private set
            {
                if (value == null)
                {
                    Set<Entity>(Entity.Empty, ref _type);
                }
                else
                {
                    Set<Entity>(value, ref _type);
                }
            }
        }
        public object Value
        {
            get { return Get<object>(ref _value); }
            set
            {
                if (value == null)
                {
                    this.Type = Entity.Empty;
                }
                else
                {
                    Entity metadata = Entity.GetMetadataType(value.GetType());
                    if (metadata == Entity.Object)
                    {
                        metadata = ((ReferenceProxy)value).Type; // !!! ReferenceProxy !!!
                    }
                    if (metadata != _type)
                    {
                        this.Type = metadata;
                    }
                }
                Set<object>(value, ref _value);
            }
        }

        protected override void UpdateKeyValues()
        {
            _article_old = _article;
            _property_old = _property;
        }
    }
}
