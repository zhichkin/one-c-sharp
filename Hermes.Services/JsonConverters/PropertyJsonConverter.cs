using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using Zhichkin.Metadata.Model;
using Zhichkin.ORM;

namespace Zhichkin.Hermes.Services
{
    public sealed class PropertyJsonConverter : JsonConverter<Property>
    {
        public override void WriteJson(JsonWriter writer, Property value, JsonSerializer serializer)
        {
            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            writer.WriteStartObject();

            writer.WritePropertyName("$id");
            serializer.Serialize(writer, new Guid(resolver.GetReference(null, value)));

            writer.WritePropertyName("$type");
            serializer.Serialize(writer, nameof(Property));

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
}
