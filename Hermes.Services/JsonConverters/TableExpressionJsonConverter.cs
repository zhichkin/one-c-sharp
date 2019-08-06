using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.Services
{
    public sealed class TableExpressionJsonConverter : JsonConverter<TableExpression>
    {
        public override void WriteJson(JsonWriter writer, TableExpression value, JsonSerializer serializer)
        {
            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            writer.WriteStartObject();

            writer.WritePropertyName("$id");
            serializer.Serialize(writer, new Guid(resolver.GetReference(null, value)));

            writer.WritePropertyName("$type");
            serializer.Serialize(writer, nameof(TableExpression));

            writer.WritePropertyName("Consumer");
            if (value.Consumer == null)
            {
                serializer.Serialize(writer, null);
            }
            else
            {
                serializer.Serialize(writer, value.Consumer, value.Consumer.GetType());
            }

            writer.WritePropertyName("Name");
            serializer.Serialize(writer, value.Name);

            writer.WritePropertyName("Alias");
            serializer.Serialize(writer, value.Alias);

            writer.WritePropertyName("Entity");
            serializer.Serialize(writer, value.Entity);

            writer.WritePropertyName("Hint");
            serializer.Serialize(writer, value.Hint);

            writer.WriteEndObject();
        }
        public override TableExpression ReadJson(JsonReader reader, Type objectType, TableExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            JObject json = JObject.Load(reader);
            JProperty property = json.Properties().Where(p => p.Name == "identity").FirstOrDefault();
            Guid identity = new Guid((string)property.Value);

            return new TableExpression(null, null);
        }
    }
}
