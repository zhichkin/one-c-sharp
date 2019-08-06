using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace Zhichkin.Hermes.Services
{
    public class QueryReferenceResolver : IReferenceResolver
    {
        private readonly IDictionary<Guid, object> _map_id_to_object = new Dictionary<Guid, object>();
        private readonly IDictionary<object, Guid> _map_object_to_id = new Dictionary<object, Guid>();

        public bool IsReferenced(object context, object value)
        {
            return _map_object_to_id.ContainsKey(value);
        }
        public string GetReference(object context, object value)
        {
            Guid id;
            if (_map_object_to_id.TryGetValue(value, out id))
            {
                return id.ToString();
            }

            id = Guid.NewGuid();
            _map_object_to_id.Add(value, id);

            return id.ToString();
        }

        public object ResolveReference(object context, string reference)
        {
            Guid id = new Guid(reference);

            object value;
            _map_id_to_object.TryGetValue(id, out value);

            return value;
        }
        public void AddReference(object context, string reference, object value)
        {
            Guid id = new Guid(reference);
            _map_id_to_object[id] = value;
        }
    }
}
