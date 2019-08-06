using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.Services
{
    public sealed class SelectStatementJsonConverter : JsonConverter<SelectStatement>
    {
        public override void WriteJson(JsonWriter writer, SelectStatement value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("$id");
            serializer.Serialize(writer, (new Guid()).ToString());

            writer.WritePropertyName("$type");
            serializer.Serialize(writer, "SelectStatement");

            writer.WritePropertyName("Consumer");
            if (value.Consumer == null)
            {
                serializer.Serialize(writer, null);
            }
            else
            {
                writer.WriteStartObject();
                writer.WritePropertyName("$ref");
                serializer.Serialize(writer, new Guid());
                writer.WriteEndObject();
            }

            writer.WritePropertyName("Name");
            serializer.Serialize(writer, value.Name);

            writer.WritePropertyName("Alias");
            serializer.Serialize(writer, value.Alias);

            writer.WritePropertyName("Entity");
            serializer.Serialize(writer, value.Entity, typeof(Entity));

            writer.WritePropertyName("Hint");
            serializer.Serialize(writer, value.Hint);

            writer.WritePropertyName("SELECT");
            if (value.SELECT == null)
            {
                serializer.Serialize(writer, null);
            }
            else if (value.SELECT.Count == 0)
            {
                writer.WriteStartArray();
                writer.WriteEndArray();
            }
            else
            {
                writer.WriteStartArray();
                foreach (PropertyExpression model in value.SELECT)
                {
                    serializer.Serialize(writer, model, model.GetType());
                }
                writer.WriteEndArray();
            }

            writer.WritePropertyName("FROM");
            if (value.FROM == null)
            {
                serializer.Serialize(writer, null);
            }
            else if (value.FROM.Count == 0)
            {
                writer.WriteStartArray();
                writer.WriteEndArray();
            }
            else
            {
                writer.WriteStartArray();
                foreach (TableExpression model in value.FROM)
                {
                    serializer.Serialize(writer, model, model.GetType());
                }
                writer.WriteEndArray();
            }

            writer.WritePropertyName("WHERE");
            if (value.WHERE == null)
            {
                serializer.Serialize(writer, null);
            }
            else
            {
                serializer.Serialize(writer, value.WHERE, value.WHERE.GetType());
            }

            writer.WriteEndObject();
        }
        public override SelectStatement ReadJson(JsonReader reader, Type objectType, SelectStatement existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            JObject json = JObject.Load(reader);
            JProperty property = json.Properties().Where(p => p.Name == "identity").FirstOrDefault();
            Guid identity = new Guid((string)property.Value);

            return new SelectStatement(null, null);
        }
    }
}
