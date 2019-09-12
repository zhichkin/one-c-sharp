namespace OneCSharp.Query.Model
{
    public sealed class QueryProcedure : QueryExpression
    {
        public QueryProcedure()
        {
            this.Keyword = Keywords.PROCEDURE;
        }
        public string Name { get; set; }
        public QueryExpressionsChildren Parameters { get; set; }
        public QueryExpressionsChildren Expressions { get; set; }
    }
}
