using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using Zhichkin.Metadata.Model;
using Zhichkin.ORM;

namespace Zhichkin.Hermes.Services
{
    public sealed class EntityJsonConverter : JsonConverter<Entity>
    {
        public override void WriteJson(JsonWriter writer, Entity value, JsonSerializer serializer)
        {
            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            writer.WriteStartObject();

            writer.WritePropertyName("$id");
            serializer.Serialize(writer, new Guid(resolver.GetReference(null, value)));

            writer.WritePropertyName("$type");
            serializer.Serialize(writer, nameof(Entity));

            writer.WritePropertyName("identity");
            serializer.Serialize(writer, value.Identity.ToString());

            writer.WriteEndObject();
        }
        public override Entity ReadJson(JsonReader reader, Type objectType, Entity existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            IReferenceResolver resolver = serializer.Context.Context as IReferenceResolver;

            Entity target = null;
            string id = string.Empty;

            JObject json = JObject.Load(reader);
            foreach (JProperty property in json.Properties())
            {
                if (property.Name == "$ref")
                {
                    target = (Entity)serializer.Deserialize(json.CreateReader());
                }
                else if (property.Name == "$id")
                {
                    id = (string)property.Value;
                }
                else if (property.Name == "identity")
                {
                    Guid identity = new Guid((string)property.Value);
                    IReferenceObjectFactory factory = MetadataPersistentContext.Current.Factory;
                    target = factory.New<Entity>(identity);
                    resolver.AddReference(null, id, target);
                }
            }
            return target;
        }
    }
}
