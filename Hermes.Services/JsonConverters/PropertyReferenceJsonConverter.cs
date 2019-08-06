using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.Services
{
    public sealed class PropertyReferenceJsonConverter : JsonConverter<PropertyReference>
    {
        public override void WriteJson(JsonWriter writer, PropertyReference value, JsonSerializer serializer)
        {
            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            writer.WriteStartObject();

            writer.WritePropertyName("$id");
            serializer.Serialize(writer, new Guid(resolver.GetReference(null, value)));

            writer.WritePropertyName("$type");
            serializer.Serialize(writer, nameof(PropertyReference));

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

            writer.WritePropertyName("Table");
            if (value.Table == null)
            {
                serializer.Serialize(writer, null);
            }
            else
            {
                serializer.Serialize(writer, value.Table, value.Table.GetType());
            }

            writer.WritePropertyName("Property");
            serializer.Serialize(writer, value.Property);

            writer.WriteEndObject();
        }
        public override PropertyReference ReadJson(JsonReader reader, Type objectType, PropertyReference existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            JObject json = JObject.Load(reader);

            string name = string.Empty;
            TableExpression table = null;
            Property property = null;

            foreach (JProperty p in json.Properties())
            {
                if (property.Name == "Name")
                {
                    name = (string)p.Value;
                }
                else if (property.Name == "Table")
                {
                    table = serializer.Deserialize<TableExpression>(p.Value.CreateReader());
                }
                else if (property.Name == "Property")
                {
                    property = serializer.Deserialize<Property>(p.Value.CreateReader());
                }
            }

            return new PropertyReference(null, table, property);
        }
    }
}
