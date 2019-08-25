using System;
using System.Collections.Generic;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.Model
{
    public class MetadataNode
    {
        public MetadataNode(EntityBase metadata)
        {
            if (metadata == null) throw new ArgumentNullException("metadata");
            this.Metadata = metadata;
        }
        public EntityBase Metadata { get; set; }
        public MetadataNode Parent { get; set; }
        public List<MetadataNode> Children { get; set; }
        public string Name { get { return this.Metadata.Name; } }
    }
}
