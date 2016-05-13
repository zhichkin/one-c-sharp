﻿using System;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

using Zhichkin.ORM;

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

    public sealed partial class Field : EntityBase
    {
        private static readonly IDataMapper _mapper = new Field.DataMapper(
            PersistentContext.ConnectionString,
            new ReferenceObjectFactory(PersistentContext.TypeCodes));

        public Field() : base(_mapper) { }
        public Field(Guid identity) : base(_mapper, identity) { }
        public Field(Guid identity, PersistentState state) : base(_mapper, identity, state) { }

        private Table table = null; // Table - owner of the field
        private Property property = null; // Property using field to store data
        private FieldPurpose purpose = FieldPurpose.Value; // purpose of the field
        private string type_name = string.Empty; // data type name
        private int length = 0;
        private int precision = 0;
        private int scale = 0;
        private bool is_nullable = false;
        private bool is_primary_key = false;
        private byte key_ordinal = 0;
        
        public Table Table { set { Set<Table>(value, ref table); } get { return Get<Table>(ref table); } }
        public Property Property { set { Set<Property>(value, ref property); } get { return Get<Property>(ref property); } }
        public FieldPurpose Purpose { set { Set<FieldPurpose>(value, ref purpose); } get { return Get<FieldPurpose>(ref purpose); } }
        public string TypeName { set { Set<string>(value, ref type_name); } get { return Get<string>(ref type_name); } }
        public int Length { set { Set<int>(value, ref length); } get { return Get<int>(ref length); } }
        public int Precision { set { Set<int>(value, ref precision); } get { return Get<int>(ref precision); } }
        public int Scale { set { Set<int>(value, ref scale); } get { return Get<int>(ref scale); } }
        public bool IsNullable { set { Set<bool>(value, ref is_nullable); } get { return Get<bool>(ref is_nullable); } }
        public bool IsPrimaryKey { set { Set<bool>(value, ref is_primary_key); } get { return Get<bool>(ref is_primary_key); } }
        public byte KeyOrdinal { set { Set<byte>(value, ref key_ordinal); } get { return Get<byte>(ref key_ordinal); } }
    }
}