using System;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public sealed partial class Relation : ValueObject
    {
        private static readonly IDataMapper _mapper = MetadataPersistentContext.Current.GetDataMapper(typeof(Relation));

        public Relation() : base(_mapper) { }
        public Relation(PersistentState state) : base(_mapper, state) { }

        private Property old_property = null;
        private Entity old_type = null;

        private Property property = null;
        private Entity type = null;

        public Property Property { set { Set<Property>(value, ref property); } get { return Get<Property>(ref property); } }
        public Entity Type { set { Set<Entity>(value, ref type); } get { return Get<Entity>(ref type); } }

        protected override void UpdateKeyValues()
        {
            old_property = property;
            old_type = type;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})",
                this.Property == null ? string.Empty : this.Property.Name,
                this.Type == null ? string.Empty : this.Type.FullName);
        }

        public int CompareTo(Relation other)
        {
            if (other == null) return 1;
            if (this.Type == null) return -1;
            if (other.Type == null) return 1;
            return this.Type.FullName.CompareTo(other.Type.FullName);
        }
    }
}