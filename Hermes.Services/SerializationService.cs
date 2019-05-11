using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.Services
{
    public interface ISerializationService
    {
        string ToSQL(QueryExpression query);
        string ToXML(QueryExpression query);
        QueryExpression FromXML(string xml);
        string ToJson(QueryExpression query);
        QueryExpression FromJson(string json);
    }
    public sealed class SerializationService : ISerializationService
    {
        public string ToXML(QueryExpression query)
        {
            return string.Empty; 
        }
        public QueryExpression FromXML(string xml)
        {
            return null;
        }
        public string ToJson(QueryExpression query)
        {
            return string.Empty;
        }
        public QueryExpression FromJson(string json)
        {
            return null;
        }
        public string ToSQL(QueryExpression query)
        {
            return string.Empty;
        }
    }
}
