using Zhichkin.Metadata.Model;

namespace OneCSharp.Query.Model
{
    public sealed class QueryParameter : QueryExpression
    {
        public QueryParameter(QueryExpressionsList<QueryParameter> consumer) : base(consumer) { }
        public string Name { get; set; }
        public Entity Type { get; set; }
        public object Value { get; set; }
    }
}
