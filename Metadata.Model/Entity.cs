using System;
using System.Collections.Generic;
using Zhichkin.Metadata.Services;

using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public sealed partial class Entity : EntityBase
    {
        private static readonly IDataMapper _mapper = MetadataPersistentContext.Current.GetDataMapper(typeof(Entity));
        private static readonly IMetadataService service = new MetadataService();

        public Entity() : base(_mapper) { }
        public Entity(Guid identity) : base(_mapper, identity) { }
        public Entity(Guid identity, PersistentState state) : base(_mapper, identity, state) { }

        private int code = 0; // type code
        private Namespace _namespace = null; // Namespace
        private Entity owner = null; // Nesting, aggregation
        private Entity parent = null; // Inheritance

        ///<summary>Type code of the entity</summary>
        public int Code { set { Set<int>(value, ref code); } get { return Get<int>(ref code); } }
        public Namespace Namespace { set { Set<Namespace>(value, ref _namespace); } get { return Get<Namespace>(ref _namespace); } }
        ///<summary>Nesting entity reference</summary>
        public Entity Owner { set { Set<Entity>(value, ref owner); } get { return Get<Entity>(ref owner); } }
        ///<summary>Inheritance: base entity reference</summary>
        public Entity Parent { set { Set<Entity>(value, ref parent); } get { return Get<Entity>(ref parent); } }
        
        public string FullName { get { return string.Format("{0}.{1}", this.Namespace.Name, this.Name); } }

        public Table MainTable
        {
            get
            {
                foreach (Table table in Tables)
                {
                    if (table.Purpose == TablePurpose.Main)
                    {
                        return table;
                    }
                }
                return null;
            }
        }

        private List<Property> properties = new List<Property>();
        private List<Entity> nestedEntities = new List<Entity>();
        private List<Table> tables = new List<Table>();

        public IList<Property> Properties
        {
            get
            {
                if (this.state == PersistentState.New) return properties;
                if (properties.Count > 0) return properties;
                return service.GetChildren<Entity, Property>(this, "entity");
            }
        }
        public IList<Entity> NestedEntities
        {
            get
            {
                if (this.state == PersistentState.New) return nestedEntities;
                if (nestedEntities.Count > 0) return nestedEntities;
                return service.GetChildren<Entity, Entity>(this, "owner");
            }
        }
        public IList<Table> Tables
        {
            get
            {
                if (this.state == PersistentState.New) return tables;
                if (tables.Count > 0) return tables;
                return service.GetChildren<Entity, Table>(this, "entity");
            }
        }
    }
}