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
        public string ToJson(QueryExpression query)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
            settings.Converters.Add(new EntityJsonConverter());
            settings.Converters.Add(new PropertyJsonConverter());
            settings.Converters.Add(new ReferenceProxyJsonConverter());
            settings.Converters.Add(new BooleanFunctionJsonConverter());
            settings.Converters.Add(new PropertyReferenceJsonConverter());
            settings.Converters.Add(new ParameterExpressionJsonConverter());
            return JsonConvert.SerializeObject(query, settings);
        }
        public QueryExpression FromJson(string json)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
            settings.Converters.Add(new EntityJsonConverter());
            settings.Converters.Add(new PropertyJsonConverter());
            settings.Converters.Add(new ReferenceProxyJsonConverter());
            settings.Converters.Add(new BooleanFunctionJsonConverter());
            settings.Converters.Add(new PropertyReferenceJsonConverter());
            settings.Converters.Add(new ParameterExpressionJsonConverter());
            return JsonConvert.DeserializeObject<QueryExpression>(json, settings);
        }
    }
}
