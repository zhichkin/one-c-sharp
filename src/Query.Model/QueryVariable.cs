using Zhichkin.Metadata.Model;

namespace OneCSharp.Query.Model
{
    public sealed class QueryVariable : QueryExpression
    {
        public QueryVariable()
        {
            this.Keyword = Keywords.DECLARE;
        }
        public string Name { get; set; }
        public Entity Type { get; set; }
        public object Value { get; set; }
    }
}
