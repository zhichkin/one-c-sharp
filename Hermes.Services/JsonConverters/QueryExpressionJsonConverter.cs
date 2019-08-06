using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.Services
{
    public sealed class QueryExpressionJsonConverter : JsonConverter<QueryExpression>
    {
        public override void WriteJson(JsonWriter writer, QueryExpression value, JsonSerializer serializer)
        {
            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            writer.WriteStartObject();

            writer.WritePropertyName("$id");
            serializer.Serialize(writer, new Guid(resolver.GetReference(null, value)));

            writer.WritePropertyName("$type");
            serializer.Serialize(writer, nameof(QueryExpression));

            writer.WritePropertyName("Consumer");
            serializer.Serialize(writer, null);

            writer.WritePropertyName("Parameters");
            if (value.Parameters == null)
            {
                serializer.Serialize(writer, null);
            }
            else if (value.Parameters.Count == 0)
            {
                writer.WriteStartArray();
                writer.WriteEndArray();
            }
            else
            {
                writer.WriteStartArray();
                foreach (ParameterExpression parameter in value.Parameters)
                {
                    serializer.Serialize(writer, parameter, typeof(ParameterExpression));
                }
                writer.WriteEndArray();
            }

            writer.WritePropertyName("Expressions");
            if (value.Expressions == null)
            {
                serializer.Serialize(writer, null);
            }
            else if (value.Expressions.Count == 0)
            {
                writer.WriteStartArray();
                writer.WriteEndArray();
            }
            else
            {
                writer.WriteStartArray();
                foreach (HermesModel model in value.Expressions)
                {
                    serializer.Serialize(writer, model, model.GetType());
                }
                writer.WriteEndArray();
            }
            
            writer.WriteEndObject();
        }
        public override QueryExpression ReadJson(JsonReader reader, Type objectType, QueryExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            QueryExpression target = new QueryExpression();

            JObject json = JObject.Load(reader);

            foreach (JProperty property in json.Properties())
            {
                if (property.Name == "$id")
                {
                    string id = (string)serializer.Deserialize(property.Value.CreateReader());
                    resolver.AddReference(null, id, target);
                }
                else if (property.Name == "Parameters")
                {
                    target.Parameters = (List<ParameterExpression>)serializer.Deserialize(property.Value.CreateReader());
                }
                else if (property.Name == "Expressions")
                {
                    JArray array = (JArray)serializer.Deserialize(property.Value.CreateReader());
                    if (array == null)
                    {
                        target.Expressions = null;
                    }
                    else if (array.Count == 0)
                    {
                        target.Expressions = new List<HermesModel>();
                    }
                    else
                    {
                        target.Expressions = new List<HermesModel>();
                        foreach (JObject item in array)
                        {
                            string typeName = (string)serializer.Deserialize(item["$type"].CreateReader());
                            //string typeName = (string)serializer.Deserialize(item.Properties().Where(p => p.Name == "$type").FirstOrDefault().Value.CreateReader());
                            Type type = serializer.SerializationBinder.BindToType(null, typeName);
                            target.Expressions.Add((HermesModel)serializer.Deserialize(item.CreateReader(), type));
                        }
                    }
                }
            }

            return target;
        }
    }
}
