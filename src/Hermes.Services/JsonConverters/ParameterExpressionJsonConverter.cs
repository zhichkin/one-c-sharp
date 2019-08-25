using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.Services
{
    public sealed class ParameterExpressionJsonConverter : JsonConverter<ParameterExpression>
    {
        public override void WriteJson(JsonWriter writer, ParameterExpression value, JsonSerializer serializer)
        {
            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            writer.WriteStartObject();

            writer.WritePropertyName("$id");
            serializer.Serialize(writer, new Guid(resolver.GetReference(null, value)));

            writer.WritePropertyName("$type");
            serializer.Serialize(writer, nameof(ParameterExpression));

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

            writer.WritePropertyName("Type");
            serializer.Serialize(writer, value.Type);

            writer.WritePropertyName("Value");
            if (value.Value == null)
            {
                serializer.Serialize(writer, null);
            }
            else
            {
                serializer.Serialize(writer, value.Value, value.Value.GetType());
            }

            writer.WriteEndObject();
        }
        public override ParameterExpression ReadJson(JsonReader reader, Type objectType, ParameterExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            ParameterExpression target = new ParameterExpression(null);

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
                else if (property.Name == "Name")
                {
                    target.Name = (string)property.Value;
                }
                else if (property.Name == "Type")
                {
                    target.Type = serializer.Deserialize<Entity>(property.Value.CreateReader());
                }
                else if (property.Name == "Value")
                {
                    if (property.Value.Type == JTokenType.Object)
                    {
                        target.Value = serializer.Deserialize<ReferenceProxy>(property.Value.CreateReader());
                    }
                    else
                    {
                        target.Value = serializer.Deserialize(property.Value.CreateReader());
                    }
                }
            }

            return target;
        }
    }
}
