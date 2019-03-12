using System;
using System.Collections.Generic;
using Zhichkin.Hermes.Infrastructure;
using Zhichkin.Metadata.Services;

using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public sealed partial class Property : EntityBase, IPropertyInfo
    {
        private static readonly IReferenceObjectFactory factory = MetadataPersistentContext.Current.Factory;
        private static readonly IDataMapper _mapper = MetadataPersistentContext.Current.GetDataMapper(typeof(Property));
        private static readonly IMetadataService service = new MetadataService();

        public Property() : base(_mapper) { }
        public Property(Guid identity) : base(_mapper, identity) { }
        public Property(Guid identity, PersistentState state) : base(_mapper, identity, state) { }

        private Entity entity = null; // Entity - owner of the property
        private PropertyPurpose purpose = PropertyPurpose.Property; // purpose of the property
        private int ordinal = 0;

        public Entity Entity { set { Set<Entity>(value, ref entity); } get { return Get<Entity>(ref entity); } }
        public PropertyPurpose Purpose { set { Set<PropertyPurpose>(value, ref purpose); } get { return Get<PropertyPurpose>(ref purpose); } }
        public int Ordinal { set { Set<int>(value, ref ordinal); } get { return Get<int>(ref ordinal); } }

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

        IEntityInfo IPropertyInfo.Entity { get { return this.Entity; } set { this.Entity = (Entity)value; } }
        IList<IFieldInfo> IPropertyInfo.Fields
        {
            get
            {
                List<IFieldInfo> list = new List<IFieldInfo>();
                foreach (Field f in this.Fields)
                {
                    list.Add(f);
                }
                return list;
            }
        }
        IList<IEntityInfo> IPropertyInfo.Types
        {
            get
            {
                List<IEntityInfo> list = new List<IEntityInfo>();
                foreach (Relation r in this.Relations)
                {
                    list.Add(r.Entity);
                }
                return list;
            }
        }
    }
}