using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.Services
{
    public sealed class PropertyExpressionJsonConverter : JsonConverter<PropertyExpression>
    {
        public override void WriteJson(JsonWriter writer, PropertyExpression value, JsonSerializer serializer)
        {
            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            writer.WriteStartObject();

            writer.WritePropertyName("$id");
            serializer.Serialize(writer, new Guid(resolver.GetReference(null, value)));

            writer.WritePropertyName("$type");
            serializer.Serialize(writer, nameof(PropertyExpression));

            writer.WritePropertyName("Consumer");
            if (value.Consumer == null)
            {
                serializer.Serialize(writer, null);
            }
            else
            {
                serializer.Serialize(writer, value.Consumer, value.Consumer.GetType());
            }

            writer.WritePropertyName("Alias");
            serializer.Serialize(writer, value.Alias);


            writer.WritePropertyName("Expression");
            if (value.Expression == null)
            {
                serializer.Serialize(writer, null);
            }
            else
            {
                serializer.Serialize(writer, value.Expression, value.Expression.GetType());
            }

            writer.WriteEndObject();
        }
        public override PropertyExpression ReadJson(JsonReader reader, Type objectType, PropertyExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            JObject json = JObject.Load(reader);

            PropertyExpression target = new PropertyExpression(null);

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
                    target.Alias = (string)property.Value;
                }
                else if (property.Name == "Expression")
                {
                    JObject expression = JObject.Load(property.Value.CreateReader());

                    JProperty refProperty = expression.Properties().Where(p => p.Name == "$ref").FirstOrDefault();
                    if (refProperty != null)
                    {
                        target.Expression = (HermesModel)serializer.Deserialize(property.Value.CreateReader());
                    }
                    else
                    {
                        JProperty typeProperty = expression.Properties().Where(p => p.Name == "$type").FirstOrDefault();
                        string typeName = (string)serializer.Deserialize(typeProperty.Value.CreateReader());
                        Type type = serializer.SerializationBinder.BindToType(null, typeName);
                        target.Expression = (HermesModel)serializer.Deserialize(property.Value.CreateReader(), type);
                    }
                }
            }

            return target;
        }
    }
}
