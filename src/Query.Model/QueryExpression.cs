namespace OneCSharp.Query.Model
{
    public abstract class QueryExpression
    {
        public QueryExpression(QueryExpression consumer)
        {
            this.Consumer = consumer;
        }
        public QueryExpression Consumer { get; set; }
    }
}
