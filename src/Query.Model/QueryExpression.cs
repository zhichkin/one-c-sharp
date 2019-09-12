namespace OneCSharp.Query.Model
{
    public abstract class QueryExpression
    {
        public QueryExpression() { }
        public QueryExpression Parent { get; set; }
        public string Keyword { get; set; } = Keywords.NONE;
    }
    public static class Keywords
    {
        public const string NONE = "";
        public const string PROCEDURE = "PROCEDURE";
        public const string FUNCTION = "FUNCTION";
        public const string DECLARE = "DECLARE";
        public const string SELECT = "SELECT";
        public const string WHERE = "WHERE";
        public const string FROM = "FROM";
        public const string ON = "ON";
    }
}
