using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
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

            JObject json = JObject.Load(reader);

            JProperty property = json.Properties().Where(p => p.Name == "identity").FirstOrDefault();
            Guid identity = new Guid((string)property.Value);

            property = json.Properties().Where(p => p.Name == "type").FirstOrDefault();
            Entity entity = serializer.Deserialize<Entity>(property.Value.CreateReader());

            return new ReferenceProxy(entity, identity);
        }
    }
}
