using System;
using System.Collections.Generic;
using Zhichkin.Metadata.Services;

using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public sealed partial class Table : EntityBase
    {
        private static readonly IDataMapper _mapper = MetadataPersistentContext.Current.GetDataMapper(typeof(Table));
        private static readonly IMetadataService service = new MetadataService();

        public Table() : base(_mapper) { }
        public Table(Guid identity) : base(_mapper, identity) { }
        public Table(Guid identity, PersistentState state) : base(_mapper, identity, state) { }

        private string schema = string.Empty;
        private Entity entity = null; // Entity - owner of the table
        private TablePurpose purpose = TablePurpose.Main; // purpose of the table

        public string Schema { set { Set<string>(value, ref schema); } get { return Get<string>(ref schema); } }
        public Entity Entity { set { Set<Entity>(value, ref entity); } get { return Get<Entity>(ref entity); } }
        public TablePurpose Purpose { set { Set<TablePurpose>(value, ref purpose); } get { return Get<TablePurpose>(ref purpose); } }
        private List<Field> fields = new List<Field>();
        public IList<Field> Fields
        {
            get
            {
                if (this.state == PersistentState.New) return fields;
                if (fields.Count > 0) return fields;
                return service.GetChildren<Table, Field>(this, "table");
            }
        }
        public string FullName
        {
            get
            {
                string tableName = string.Empty;

                string DatabaseName = this.Entity.InfoBase.Database;

                if (string.IsNullOrWhiteSpace(DatabaseName))
                {
                    if (string.IsNullOrWhiteSpace(this.Schema))
                    {
                        tableName = string.Format("[{0}]", this.Name);
                    }
                    else
                    {
                        tableName = string.Format("[{0}].[{1}]", this.Schema, this.Name);
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(this.Schema))
                    {
                        tableName = string.Format("[{0}].[dbo].[{1}]", DatabaseName, this.Name);
                    }
                    else
                    {
                        tableName = string.Format("[{0}].[{1}].[{2}]", DatabaseName, this.Schema, this.Name);
                    }
                }

                return tableName;
            }
        }
    }
}