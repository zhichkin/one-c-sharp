using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.Services
{
    public sealed class SelectStatementJsonConverter : JsonConverter<SelectStatement>
    {
        public override void WriteJson(JsonWriter writer, SelectStatement value, JsonSerializer serializer)
        {
            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            writer.WriteStartObject();

            writer.WritePropertyName("$id");
            serializer.Serialize(writer, new Guid(resolver.GetReference(null, value)));

            writer.WritePropertyName("$type");
            serializer.Serialize(writer, nameof(SelectStatement));

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

            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            SelectStatement target = new SelectStatement(null, null);

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
                else if (property.Name == "SELECT")
                {
                    DeserializeSELECT(serializer, property, target);
                }
                else if (property.Name == "FROM")
                {
                    DeserializeFROM(serializer, property, target);
                }
                else if (property.Name == "WHERE")
                {
                    // TODO !!!
                }
            }

            return target;
        }
        private void DeserializeSELECT(JsonSerializer serializer, JProperty property, SelectStatement target)
        {
            JArray array = (JArray)serializer.Deserialize(property.Value.CreateReader());
            if (array == null)
            {
                target.SELECT = null;
            }
            else if (array.Count == 0)
            {
                target.SELECT = new List<PropertyExpression>();
            }
            else
            {
                target.SELECT = new List<PropertyExpression>();
                foreach (JObject item in array)
                {
                    target.SELECT.Add(serializer.Deserialize<PropertyExpression>(item.CreateReader()));
                }
            }
        }
        private void DeserializeFROM(JsonSerializer serializer, JProperty property, SelectStatement target)
        {
            JArray array = (JArray)serializer.Deserialize(property.Value.CreateReader());
            if (array == null)
            {
                target.FROM = null;
            }
            else if (array.Count == 0)
            {
                target.FROM = new List<TableExpression>();
            }
            else
            {
                target.FROM = new List<TableExpression>();
                foreach (JObject item in array)
                {
                    JObject expression = JObject.Load(item.CreateReader());

                    JProperty refProperty = expression.Properties().Where(p => p.Name == "$ref").FirstOrDefault();
                    if (refProperty != null)
                    {
                        target.FROM.Add((TableExpression)serializer.Deserialize(item.CreateReader()));
                    }
                    else
                    {
                        JProperty typeProperty = expression.Properties().Where(p => p.Name == "$type").FirstOrDefault();
                        string typeName = (string)serializer.Deserialize(typeProperty.Value.CreateReader());
                        Type type = serializer.SerializationBinder.BindToType(null, typeName);
                        target.FROM.Add((TableExpression)serializer.Deserialize(item.CreateReader(), type));
                    }
                }
            }
        }
    }
}
