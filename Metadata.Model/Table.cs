using System;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public enum TablePurpose
    {
        /// <summary>The main table to store data.</summary>
        Main,
        /// <summary>The table part of entity.</summary>
        TablePart,
        /// <summary>The totals table.</summary>
        Totals,
        /// <summary>The settings of the entity.</summary>
        Settings
    }

    public sealed partial class Table : EntityBase
    {
        private static readonly IDataMapper _mapper = MetadataPersistentContext.Current.GetDataMapper(typeof(Table));

        public Table() : base(_mapper) { }
        public Table(Guid identity) : base(_mapper, identity) { }
        public Table(Guid identity, PersistentState state) : base(_mapper, identity, state) { }

        private string schema = string.Empty;
        private Entity entity = null; // Entity - owner of the table
        private TablePurpose purpose = TablePurpose.Main; // purpose of the table

        public string Schema { set { Set<string>(value, ref schema); } get { return Get<string>(ref schema); } }
        public Entity Entity { set { Set<Entity>(value, ref entity); } get { return Get<Entity>(ref entity); } }
        public TablePurpose Purpose { set { Set<TablePurpose>(value, ref purpose); } get { return Get<TablePurpose>(ref purpose); } }
    }
}