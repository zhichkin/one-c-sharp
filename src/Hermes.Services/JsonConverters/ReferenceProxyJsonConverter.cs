﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.Services
{
    public sealed class ReferenceProxyJsonConverter : JsonConverter<ReferenceProxy>
    {
        public override void WriteJson(JsonWriter writer, ReferenceProxy value, JsonSerializer serializer)
        {
            ReferenceProxy proxy = value as ReferenceProxy;
            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            writer.WriteStartObject();

            writer.WritePropertyName("$id");
            serializer.Serialize(writer, new Guid(resolver.GetReference(null, value)));

            writer.WritePropertyName("$type");
            serializer.Serialize(writer, nameof(ReferenceProxy));

            writer.WritePropertyName("type");
            serializer.Serialize(writer, proxy.Type);

            writer.WritePropertyName("identity");
            serializer.Serialize(writer, proxy.Identity.ToString());

            writer.WriteEndObject();
        }
        public override ReferenceProxy ReadJson(JsonReader reader, Type objectType, ReferenceProxy existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            ReferenceProxy target = null;
            string id = string.Empty;
            Entity entity = null;

            JObject json = JObject.Load(reader);
            foreach (JProperty property in json.Properties())
            {
                if (property.Name == "$ref")
                {
                    target = (ReferenceProxy)serializer.Deserialize(json.CreateReader());
                }
                else if (property.Name == "$id")
                {
                    id = (string)property.Value;
                }
                else if (property.Name == "type")
                {
                   entity = serializer.Deserialize<Entity>(property.Value.CreateReader());
                }
                else if (property.Name == "identity")
                {
                    Guid identity = new Guid((string)property.Value);
                    target = new ReferenceProxy(entity, identity);
                    resolver.AddReference(null, id, target);
                }
            }
            return target;
        }
    }
}
