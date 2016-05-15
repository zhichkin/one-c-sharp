using System;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

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
        private static readonly IDataMapper _mapper = MetadataPersistentContext.Current.GetDataMapper(typeof(Property));

        public Property() : base(_mapper) { }
        public Property(Guid identity) : base(_mapper, identity) { }
        public Property(Guid identity, PersistentState state) : base(_mapper, identity, state) { }

        private Entity entity = null; // Entity - owner of the property
        private PropertyPurpose purpose = PropertyPurpose.Property; // purpose of the property

        public Entity Entity { set { Set<Entity>(value, ref entity); } get { return Get<Entity>(ref entity); } }
        public PropertyPurpose Purpose { set { Set<PropertyPurpose>(value, ref purpose); } get { return Get<PropertyPurpose>(ref purpose); } }
    }
}