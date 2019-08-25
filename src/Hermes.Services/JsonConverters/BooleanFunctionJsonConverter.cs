using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.Services
{
    public sealed class BooleanFunctionJsonConverter : JsonConverter<BooleanFunction>
    {
        public override void WriteJson(JsonWriter writer, BooleanFunction value, JsonSerializer serializer)
        {
            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            if (value is BooleanOperator)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("$id");
                serializer.Serialize(writer, new Guid(resolver.GetReference(null, value)));

                writer.WritePropertyName("$type");
                serializer.Serialize(writer, nameof(BooleanOperator));

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

                BooleanOperator source = (BooleanOperator)value;
                writer.WritePropertyName("Operands");
                if (source.Operands == null)
                {
                    serializer.Serialize(writer, null);
                }
                else if (source.Operands.Count == 0)
                {
                    writer.WriteStartArray();
                    writer.WriteEndArray();
                }
                else
                {
                    writer.WriteStartArray();
                    foreach (BooleanFunction item in source.Operands)
                    {
                        serializer.Serialize(writer, item, item.GetType());
                    }
                    writer.WriteEndArray();
                }

                writer.WriteEndObject();
            }
            else if (value is ComparisonOperator)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("$id");
                serializer.Serialize(writer, new Guid(resolver.GetReference(null, value)));

                writer.WritePropertyName("$type");
                serializer.Serialize(writer, nameof(ComparisonOperator));

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

                ComparisonOperator source = (ComparisonOperator)value;
                writer.WritePropertyName("LeftExpression");
                if (source.LeftExpression == null)
                {
                    serializer.Serialize(writer, null);
                }
                else
                {
                    serializer.Serialize(writer, source.LeftExpression, source.LeftExpression.GetType());
                }

                writer.WritePropertyName("RightExpression");
                if (source.RightExpression == null)
                {
                    serializer.Serialize(writer, null);
                }
                else
                {
                    serializer.Serialize(writer, source.RightExpression, source.RightExpression.GetType());
                }

                writer.WriteEndObject();
            }
        }

        public override BooleanFunction ReadJson(JsonReader reader, Type objectType, BooleanFunction existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            JObject json = JObject.Load(reader);

            BooleanFunction target;

            JProperty operandsProperty = json.Properties().Where(p => p.Name == "Operands").FirstOrDefault();
            if (operandsProperty == null)
            {
                target = new ComparisonOperator(null);
                DeserializeComparisonOperator(serializer, json, (ComparisonOperator)target);
            }
            else
            {
                target = new BooleanOperator(null);
                DeserializeBooleanOperator(serializer, json, (BooleanOperator)target);
            }

            return target;
        }
        private void DeserializeBooleanOperator(JsonSerializer serializer, JObject json, BooleanOperator target)
        {
            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

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
                else if (property.Name == "Operands")
                {
                    target.Operands = serializer.Deserialize<List<BooleanFunction>>(property.Value.CreateReader());
                }
            }
        }
        private void DeserializeComparisonOperator(JsonSerializer serializer, JObject json, ComparisonOperator target)
        {
            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

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
                else if (property.Name == "LeftExpression")
                {
                    JObject expression = JObject.Load(property.Value.CreateReader());
                    JProperty refProperty = expression.Properties().Where(p => p.Name == "$ref").FirstOrDefault();
                    if (refProperty != null)
                    {
                        target.LeftExpression = (HermesModel)serializer.Deserialize(property.Value.CreateReader());
                    }
                    else
                    {
                        JProperty typeProperty = expression.Properties().Where(p => p.Name == "$type").FirstOrDefault();
                        if (typeProperty == null)
                        {
                            target.LeftExpression = null;
                        }
                        else
                        {
                            string typeName = (string)serializer.Deserialize(typeProperty.Value.CreateReader());
                            Type type = serializer.SerializationBinder.BindToType(null, typeName);
                            target.LeftExpression = (HermesModel)serializer.Deserialize(property.Value.CreateReader(), type);
                        }
                    }
                }
                else if (property.Name == "RightExpression")
                {
                    JObject expression = JObject.Load(property.Value.CreateReader());
                    JProperty refProperty = expression.Properties().Where(p => p.Name == "$ref").FirstOrDefault();
                    if (refProperty != null)
                    {
                        target.RightExpression = (HermesModel)serializer.Deserialize(property.Value.CreateReader());
                    }
                    else
                    {
                        JProperty typeProperty = expression.Properties().Where(p => p.Name == "$type").FirstOrDefault();
                        if (typeProperty == null)
                        {
                            target.RightExpression = null;
                        }
                        else
                        {
                            string typeName = (string)serializer.Deserialize(typeProperty.Value.CreateReader());
                            Type type = serializer.SerializationBinder.BindToType(null, typeName);
                            target.RightExpression = (HermesModel)serializer.Deserialize(property.Value.CreateReader(), type);
                        }
                    }
                }
            }
        }
    }
}
