using System.Collections.Generic;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Metadata.UI
{
    public sealed class NamespaceViewModel
    {
        private readonly Namespace _namespace;
        private readonly List<Entity> _entities = new List<Entity>();
        public NamespaceViewModel(Namespace ns) { _namespace = ns; }
        public string Name { get { return _namespace.Name; } }
        public Namespace Model { get { return _namespace; } }
        public List<Entity> Entities { get { return _entities; } }
    }
}
