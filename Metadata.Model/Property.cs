using System;
using System.Collections.Generic;
using Zhichkin.Metadata.Services;

using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public enum PropertyPurpose
    {
        /// <summary>The property is being used by system.</summary>
        System,
        /// <summary>The property is being used as a property.</summary>
        Property,
        /// <summary>The property is being used as a dimension.</summary>
        Dimension,
        /// <summary>The property is being used as a measure.</summary>
        Measure
    }

    public sealed partial class Property : EntityBase
    {
        private static readonly IReferenceObjectFactory factory = MetadataPersistentContext.Current.Factory;
        private static readonly IDataMapper _mapper = MetadataPersistentContext.Current.GetDataMapper(typeof(Property));
        private static readonly IMetadataService service = new MetadataService();

        public Property() : base(_mapper) { }
        public Property(Guid identity) : base(_mapper, identity) { }
        public Property(Guid identity, PersistentState state) : base(_mapper, identity, state) { }

        private Entity entity = null; // Entity - owner of the property
        private PropertyPurpose purpose = PropertyPurpose.Property; // purpose of the property

        public Entity Entity { set { Set<Entity>(value, ref entity); } get { return Get<Entity>(ref entity); } }
        public PropertyPurpose Purpose { set { Set<PropertyPurpose>(value, ref purpose); } get { return Get<PropertyPurpose>(ref purpose); } }

        private List<Field> fields = new List<Field>();
        private List<Relation> relations = new List<Relation>();

        public IList<Field> Fields
        {
            get
            {
                if (this.state == PersistentState.New) return fields;
                if (fields.Count > 0) return fields;
                return service.GetChildren<Property, Field>(this, "property");
            }
        }
        public IList<Relation> Relations
        {
            get
            {
                if (this.state == PersistentState.New) return relations;
                if (relations.Count > 0) return relations;
                return service.GetChildren<Property, Relation>(this, "property", (r, e) =>
                    {
                        e.Entity = factory.New<Entity>(r.GetGuid(0));
                        e.Property = factory.New<Property>(r.GetGuid(1));
                    });
            }
        }
    }
}