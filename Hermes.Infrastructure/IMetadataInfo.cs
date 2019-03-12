using System;
using System.Collections.Generic;
using System.Text;

namespace Zhichkin.Hermes.Infrastructure
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
        Measure,
        /// <summary>This property is used to reference parent (adjacency list).</summary>
        Hierarchy
    }
    public enum FieldPurpose
    {
        /// <summary>Value of the property (default).</summary>
        Value,
        /// <summary>Helps to locate fields having [boolean, string, number, binary, datetime, object] types</summary>
        Locator,
        /// <summary>The field holds boolean value.</summary>
        Boolean,
        /// <summary>The field holds string value.</summary>
        String,
        /// <summary>The field holds numeric value.</summary>
        Number,
        /// <summary>The field holds binary value (bytes array).</summary>
        Binary,
        /// <summary>The field holds date and time value.</summary>
        DateTime,
        /// <summary>GUID value referencing primary key of an entity.</summary>
        Object,
        /// <summary>Integer code of the entity type (discriminator).</summary>
        TypeCode
    }

    public interface IMetadataInfo
    {
        Guid Identity { get; }
        string Name { set; get; }
    }
    public interface IInfoBaseInfo : IMetadataInfo
    {
        string Server { set; get; }
        string Database { set; get; }
    }
    public interface INamespaceInfo : IMetadataInfo
    {
        IInfoBaseInfo InfoBase { get; }
        INamespaceInfo Namespace { get; }
    }
    public interface IEntityInfo : IMetadataInfo
    {
        int TypeCode { get; }
        INamespaceInfo Namespace { get; set; }
        IEntityInfo Owner { get; set; }
        string FullName { get; }
        IList<IPropertyInfo> Properties { get; }
        IList<ITableInfo> Tables { get; }
    }
    public interface IPropertyInfo : IMetadataInfo
    {
        IEntityInfo Entity { get; set; }
        int Ordinal { get; set; }
        PropertyPurpose Purpose { get; set; }
        IList<IEntityInfo> Types { get; }
        IList<IFieldInfo> Fields { get; }
    }
    public interface ITableInfo : IMetadataInfo
    {
        IEntityInfo Entity { get; set; }
        string Schema { get; set; }
        IList<IFieldInfo> Fields { get; }
    }
    public interface IFieldInfo : IMetadataInfo
    {
        ITableInfo Table { get; set; }
        IPropertyInfo Property { get; set; }
        FieldPurpose Purpose { get; set; }
        string TypeName { get; set; }
        int Length { get; set; }
        int Precision { get; set; }
        int Scale { get; set; }
        bool IsNullable { get; set; }
        bool IsPrimaryKey { get; set; }
        byte KeyOrdinal { get; set; }
    }
}
