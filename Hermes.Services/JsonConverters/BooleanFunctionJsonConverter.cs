using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
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
                //Type type = typeof(ComparisonOperator);
                //string typeName = $"{type.FullName}, {type.Namespace}";

                writer.WriteStartObject();

                writer.WritePropertyName("$id");
                serializer.Serialize(writer, new Guid(resolver.GetReference(null, value)));

                writer.WritePropertyName("$type");
                serializer.Serialize(writer, nameof(ComparisonOperator));

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
