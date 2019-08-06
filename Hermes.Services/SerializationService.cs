using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization;
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
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };
        
        public SerializationService()
        {
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
            IReferenceResolver resolver = new QueryReferenceResolver();
            settings.ReferenceResolverProvider = () => { return resolver; };
            settings.Context = new StreamingContext(StreamingContextStates.Other, resolver);
            return JsonConvert.SerializeObject(query, settings);
        }
        public QueryExpression FromJson(string json)
        {
            IReferenceResolver resolver = new QueryReferenceResolver();
            settings.ReferenceResolverProvider = () => { return resolver; };
            settings.Context = new StreamingContext(StreamingContextStates.Other, resolver);
            return JsonConvert.DeserializeObject<QueryExpression>(json, settings);
        }
    }
}
