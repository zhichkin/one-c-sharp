using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.Services
{
    public sealed class QueryExpressionJsonConverter : JsonConverter<QueryExpression>
    {
        public override void WriteJson(JsonWriter writer, QueryExpression value, JsonSerializer serializer)
        {
            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            writer.WriteStartObject();

            writer.WritePropertyName("$id");
            serializer.Serialize(writer, new Guid(resolver.GetReference(null, value)));

            writer.WritePropertyName("$type");
            serializer.Serialize(writer, "QueryExpression");

            writer.WritePropertyName("Consumer");
            serializer.Serialize(writer, null);

            writer.WritePropertyName("Parameters");
            if (value.Parameters == null)
            {
                serializer.Serialize(writer, null);
            }
            else if (value.Parameters.Count == 0)
            {
                writer.WriteStartArray();
                writer.WriteEndArray();
            }
            else
            {
                writer.WriteStartArray();
                foreach (ParameterExpression parameter in value.Parameters)
                {
                    serializer.Serialize(writer, parameter, typeof(ParameterExpression));
                }
                writer.WriteEndArray();
            }

            writer.WritePropertyName("Expressions");
            if (value.Expressions == null)
            {
                serializer.Serialize(writer, null);
            }
            else if (value.Expressions.Count == 0)
            {
                writer.WriteStartArray();
                writer.WriteEndArray();
            }
            else
            {
                writer.WriteStartArray();
                foreach (HermesModel model in value.Expressions)
                {
                    serializer.Serialize(writer, model, model.GetType());
                }
                writer.WriteEndArray();
            }
            
            writer.WriteEndObject();
        }
        public override QueryExpression ReadJson(JsonReader reader, Type objectType, QueryExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            JObject json = JObject.Load(reader);
            JProperty property = json.Properties().Where(p => p.Name == "identity").FirstOrDefault();
            Guid identity = new Guid((string)property.Value);

            return new QueryExpression();
        }
    }
}
