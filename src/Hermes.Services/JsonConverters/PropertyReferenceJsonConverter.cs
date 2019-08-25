using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
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

            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            PropertyReference target = new PropertyReference(null);

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
                else if (property.Name == "Table")
                {
                    JObject expression = JObject.Load(property.Value.CreateReader());
                    JProperty refProperty = expression.Properties().Where(p => p.Name == "$ref").FirstOrDefault();
                    if (refProperty != null)
                    {
                        target.Table = (TableExpression)serializer.Deserialize(property.Value.CreateReader());
                    }
                    else
                    {
                        JProperty typeProperty = expression.Properties().Where(p => p.Name == "$type").FirstOrDefault();
                        if (typeProperty == null)
                        {
                            target.Table = null;
                        }
                        else
                        {
                            string typeName = (string)serializer.Deserialize(typeProperty.Value.CreateReader());
                            Type type = serializer.SerializationBinder.BindToType(null, typeName);
                            target.Table = (TableExpression)serializer.Deserialize(property.Value.CreateReader(), type);
                        }
                    }
                }
                else if (property.Name == "Property")
                {
                    target.Property = serializer.Deserialize<Property>(property.Value.CreateReader());
                }
            }

            return target;
        }
    }
}
