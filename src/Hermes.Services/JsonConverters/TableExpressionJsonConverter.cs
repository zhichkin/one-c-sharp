using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;

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

            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            TableExpression target = new TableExpression(null);

            JObject json = JObject.Load(reader);
            foreach (JProperty property in json.Properties())
            {
                if (property.Name == "$id")
                {
                    string id = (string)serializer.Deserialize(property.Value.CreateReader());
                    resolver.AddReference(null, id, target);
                }
                else if (property.Name == "Consumer")
                {
                    target.Consumer = (HermesModel)serializer.Deserialize(property.Value.CreateReader());
                }
                else if (property.Name == "Alias")
                {
                    target.Alias = (string)serializer.Deserialize(property.Value.CreateReader());
                }
                else if (property.Name == "Hint")
                {
                    target.Hint = (string)serializer.Deserialize(property.Value.CreateReader());
                }
                else if (property.Name == "Entity")
                {
                    target.Entity = serializer.Deserialize<Entity>(property.Value.CreateReader());
                }
            }

            return target;
        }
    }
}
