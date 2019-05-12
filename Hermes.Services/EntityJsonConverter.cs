using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;
using Zhichkin.ORM;

namespace Zhichkin.Hermes.Services
{
    public sealed class EntityJsonConverter : JsonConverter<Entity>
    {
        public override void WriteJson(JsonWriter writer, Entity value, JsonSerializer serializer)
        {
            Type type = typeof(Entity);

            writer.WriteStartObject();
            writer.WritePropertyName("$type");
            serializer.Serialize(writer, $"{type.FullName}, {type.Namespace}");
            writer.WritePropertyName("identity");
            serializer.Serialize(writer, value.Identity.ToString());
            writer.WriteEndObject();
        }
        public override Entity ReadJson(JsonReader reader, Type objectType, Entity existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            JObject json = JObject.Load(reader);
            JProperty property = json.Properties().Where(p => p.Name == "identity").FirstOrDefault();
            Guid identity = new Guid((string)property.Value);

            IReferenceObjectFactory factory = MetadataPersistentContext.Current.Factory;
            return factory.New<Entity>(identity);
        }
    }

    public sealed class PropertyJsonConverter : JsonConverter<Property>
    {
        public override void WriteJson(JsonWriter writer, Property value, JsonSerializer serializer)
        {
            Type type = typeof(Property);

            writer.WriteStartObject();
            writer.WritePropertyName("$type");
            serializer.Serialize(writer, $"{type.FullName}, {type.Namespace}");
            writer.WritePropertyName("identity");
            serializer.Serialize(writer, value.Identity.ToString());
            writer.WriteEndObject();
        }
        public override Property ReadJson(JsonReader reader, Type objectType, Property existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            JObject json = JObject.Load(reader);
            JProperty property = json.Properties().Where(p => p.Name == "identity").FirstOrDefault();
            Guid identity = new Guid((string)property.Value);

            IReferenceObjectFactory factory = MetadataPersistentContext.Current.Factory;
            return factory.New<Property>(identity);
        }
    }

    public sealed class ReferenceProxyJsonConverter : JsonConverter<ReferenceProxy>
    {
        public override void WriteJson(JsonWriter writer, ReferenceProxy value, JsonSerializer serializer)
        {
            ReferenceProxy proxy = value as ReferenceProxy;
            Type type = typeof(ReferenceProxy);

            writer.WriteStartObject();
            writer.WritePropertyName("$type");
            serializer.Serialize(writer, $"{type.FullName}, {type.Namespace}");
            writer.WritePropertyName("type");
            serializer.Serialize(writer, proxy.Type);
            writer.WritePropertyName("identity");
            serializer.Serialize(writer, proxy.Identity.ToString());
            writer.WriteEndObject();
        }
        public override ReferenceProxy ReadJson(JsonReader reader, Type objectType, ReferenceProxy existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            JObject json = JObject.Load(reader);

            JProperty property = json.Properties().Where(p => p.Name == "identity").FirstOrDefault();
            Guid identity = new Guid((string)property.Value);

            property = json.Properties().Where(p => p.Name == "type").FirstOrDefault();
            Entity entity = serializer.Deserialize<Entity>(property.Value.CreateReader());

            return new ReferenceProxy(entity, identity);
        }
    }

    public sealed class ParameterExpressionJsonConverter : JsonConverter<ParameterExpression>
    {
        public override void WriteJson(JsonWriter writer, ParameterExpression value, JsonSerializer serializer)
        {
            Type type = typeof(ParameterExpression);

            writer.WriteStartObject();
            writer.WritePropertyName("$type");
            serializer.Serialize(writer, $"{type.FullName}, {type.Namespace}");

            writer.WritePropertyName("Name");
            serializer.Serialize(writer, value.Name);

            writer.WritePropertyName("Type");
            serializer.Serialize(writer, value.Type);

            writer.WritePropertyName("Value");
            if (value.Value == null)
            {
                serializer.Serialize(writer, value.Value);
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

            JObject json = JObject.Load(reader);

            string name = string.Empty;
            Entity entity = null;
            object value = null;

            foreach (JProperty property in json.Properties())
            {
                if (property.Name == "Name")
                {
                    name = (string)property.Value;
                }
                else if (property.Name == "Type")
                {
                    entity = serializer.Deserialize<Entity>(property.Value.CreateReader());
                }
                else if (property.Name == "Value")
                {
                    if (property.Value.Type == JTokenType.Object)
                    {
                        value = serializer.Deserialize<ReferenceProxy>(property.Value.CreateReader());
                    }
                    else
                    {
                        value = serializer.Deserialize(property.Value.CreateReader());
                    }
                }
            }

            ParameterExpression parameter = new ParameterExpression(null)
            {
                Name = name,
                Type = entity,
                Value = value
            };
            return parameter;
        }
    }

    public sealed class BooleanFunctionJsonConverter : JsonConverter<BooleanFunction>
    {
        public override bool CanWrite { get { return false; } }
        public override void WriteJson(JsonWriter writer, BooleanFunction value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        public override BooleanFunction ReadJson(JsonReader reader, Type objectType, BooleanFunction existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            JObject json = JObject.Load(reader);

            string name = string.Empty;
            HermesModel leftExpression = null;
            HermesModel rightExpression = null;
            List<BooleanFunction> operands = null;

            bool isBooleanOperator = false;

            foreach (JProperty property in json.Properties())
            {
                if (property.Name == "Name")
                {
                    name = (string)property.Value;
                }
                else if (property.Name == "Operands")
                {
                    isBooleanOperator = true;
                    operands = (List<BooleanFunction>)serializer.Deserialize(property.Value.CreateReader());
                }
                else if (property.Name == "LeftExpression")
                {
                    leftExpression = (HermesModel)serializer.Deserialize(property.Value.CreateReader());
                }
                else if (property.Name == "RightExpression")
                {
                    rightExpression = serializer.Deserialize<ParameterExpression>(property.Value.CreateReader());
                }
            }

            BooleanFunction function;
            if (isBooleanOperator)
            {
                function = new BooleanOperator(null)
                {
                    Name = name,
                    Operands = operands
                };
            }
            else
            {
                function = new ComparisonOperator(null)
                {
                    Name = name,
                    LeftExpression = leftExpression,
                    RightExpression = rightExpression
                };
            }
            return function;
        }
    }
}
