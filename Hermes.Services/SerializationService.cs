using Newtonsoft.Json;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.Services
{
    public interface ISerializationService
    {
        string ToJson(QueryExpression query);
        QueryExpression FromJson(string json);
    }
    public sealed class SerializationService : ISerializationService
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        public SerializationService()
        {
            //settings.ReferenceResolverProvider = () => { return new QueryReferenceResolver(); };

            settings.Converters.Add(new QueryExpressionJsonConverter());
            settings.Converters.Add(new SelectStatementJsonConverter());
            settings.Converters.Add(new EntityJsonConverter());
            settings.Converters.Add(new PropertyJsonConverter());
            settings.Converters.Add(new ReferenceProxyJsonConverter());
            settings.Converters.Add(new BooleanFunctionJsonConverter());
            settings.Converters.Add(new PropertyReferenceJsonConverter());
            settings.Converters.Add(new ParameterExpressionJsonConverter());
        }

        public string ToJson(QueryExpression query)
        {
            return JsonConvert.SerializeObject(query, settings);
        }
        public QueryExpression FromJson(string json)
        {
            return JsonConvert.DeserializeObject<QueryExpression>(json, settings);
        }
    }
}
