namespace OneCSharp.Query.Model
{
    public sealed class QuerySyntaxTree : QueryExpression
    {
        public QuerySyntaxTree(QueryExpression consumer) : base(consumer) { }
        public QueryExpressionsList<QueryParameter> Parameters { get; set; }
        public QueryExpressionsList<QueryExpression> Expressions { get; set; }
    }
}
