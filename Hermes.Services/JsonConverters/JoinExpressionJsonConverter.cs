using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.Services
{
    public sealed class JoinExpressionJsonConverter : JsonConverter<JoinExpression>
    {
        public override void WriteJson(JsonWriter writer, JoinExpression value, JsonSerializer serializer)
        {
            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            writer.WriteStartObject();

            writer.WritePropertyName("$id");
            serializer.Serialize(writer, new Guid(resolver.GetReference(null, value)));

            writer.WritePropertyName("$type");
            serializer.Serialize(writer, nameof(JoinExpression));

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

            writer.WritePropertyName("JoinType");
            serializer.Serialize(writer, value.JoinType);

            writer.WritePropertyName("ON");
            if (value.ON == null)
            {
                serializer.Serialize(writer, null);
            }
            else
            {
                serializer.Serialize(writer, value.ON, value.ON.GetType());
            }

            writer.WriteEndObject();
        }
        public override JoinExpression ReadJson(JsonReader reader, Type objectType, JoinExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            JoinExpression target = new JoinExpression(null, null);

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
                else if (property.Name == "JoinType")
                {
                    target.JoinType = (string)serializer.Deserialize(property.Value.CreateReader());
                }
                else if (property.Name == "ON")
                {
                    target.ON = serializer.Deserialize<BooleanFunction>(property.Value.CreateReader());
                }
            }

            return target;
        }
    }
}
