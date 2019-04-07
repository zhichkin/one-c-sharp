namespace Zhichkin.Metadata.Model
{
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
}
