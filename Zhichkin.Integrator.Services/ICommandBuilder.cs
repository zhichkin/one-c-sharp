using System;
using System.Text;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using Zhichkin.ORM;
using Zhichkin.Metadata.Model;
using Zhichkin.Integrator.Model;

namespace Zhichkin.Integrator.Services
{
    public interface ICommandBuilder
    {
        InfoBase ChangeTrackingContext { set; get; }
        void PrepareInsert(IDbCommand command, IDataRecord record);
        void PrepareUpdate(IDbCommand command, IDataRecord record);
        void PrepareDelete(IDbCommand command, IDataRecord record);
    }

    public sealed class DataCommandBuilder : ICommandBuilder
    {
        private readonly Entity _metadata = null;

        public DataCommandBuilder(Entity metadata) { _metadata = metadata; }

        public InfoBase ChangeTrackingContext { set; get; }
        
        public void PrepareInsert(IDbCommand command, IDataRecord record)
        {
            command.CommandType = CommandType.Text;
            command.Parameters.Clear();
            AddChangeTrackingContext(command);

            string SQL = "WITH CHANGE_TRACKING_CONTEXT (@change_tracking_context) INSERT {0} ({1}) VALUES ({2});";
            StringBuilder fields = new StringBuilder();
            StringBuilder values = new StringBuilder();
            string table = _metadata.MainTable.FullName;

            for (int i = 0; i < record.FieldCount; i++)
            {
                Field field = _metadata.MainTable.Fields.Where(f => f.Name == record.GetName(i)).FirstOrDefault();
                if (field == null) continue; // throw new FormatException(); ???

                if (fields.Length > 0) fields.Append(", ");
                if (values.Length > 0) values.Append(", ");
                fields.AppendFormat("[{0}]", record.GetName(i));
                values.AppendFormat("@p{0}", i.ToString());
                AddCommandParameter(command, string.Format("p{0}", i.ToString()), field, record[i]);
            }

            command.CommandText = string.Format(SQL, table, fields.ToString(), values.ToString());
        }
        public void PrepareUpdate(IDbCommand command, IDataRecord record)
        {
            command.CommandType = CommandType.Text;
            command.Parameters.Clear();
            AddChangeTrackingContext(command);

            string SQL = "WITH CHANGE_TRACKING_CONTEXT (@change_tracking_context) UPDATE {0} SET {1} WHERE {2};";
            StringBuilder where = new StringBuilder();
            StringBuilder values = new StringBuilder();
            string table = _metadata.MainTable.FullName;

            for (int i = 0; i < record.FieldCount; i++)
            {
                Field field = _metadata.MainTable.Fields.Where(f => f.Name == record.GetName(i)).FirstOrDefault();
                if (field == null) continue; // throw new FormatException(); ???
                if (field.IsPrimaryKey)
                {
                    if (where.Length > 0) where.Append(" AND ");
                    where.AppendFormat("[{0}] = @p{1}", field.Name, i.ToString());
                }
                else
                {
                    if (values.Length > 0) values.Append(", ");
                    values.AppendFormat("[{0}] = @p{1}", field.Name, i.ToString());
                }
                AddCommandParameter(command, string.Format("p{0}", i.ToString()), field, record[i]);
            }

            command.CommandText = string.Format(SQL, table, values.ToString(), where.ToString());
        }
        public void PrepareDelete(IDbCommand command, IDataRecord record)
        {
            command.CommandType = CommandType.Text;
            command.Parameters.Clear();
            AddChangeTrackingContext(command);

            string SQL = "WITH CHANGE_TRACKING_CONTEXT (@change_tracking_context) DELETE {0} WHERE {1};";
            StringBuilder where = new StringBuilder();
            string table = _metadata.MainTable.FullName;

            for (int i = 0; i < record.FieldCount; i++)
            {
                Field field = _metadata.MainTable.Fields.Where(f => f.Name == record.GetName(i)).FirstOrDefault();
                if (field == null) continue; // throw new FormatException(); ???
                if (where.Length > 0) where.Append(" AND ");
                where.AppendFormat("[{0}] = @p{1}", field.Name, i.ToString());
                AddCommandParameter(command, string.Format("p{0}", i.ToString()), field, record[i]);
            }

            command.CommandText = string.Format(SQL, table, where.ToString());
        }
        private void AddCommandParameter(IDbCommand command, string name, Field field, object value)
        {
            command.Parameters.Add(new SqlParameter()
            {
                Direction = ParameterDirection.Input,
                ParameterName = name,
                SqlDbType = Helper.GetSqlTypeByName(field.TypeName),
                Value = value
            });
        }
        private void AddChangeTrackingContext(IDbCommand command)
        {
            if (ChangeTrackingContext == null) return;
            command.Parameters.Add(new SqlParameter()
            {
                Direction = ParameterDirection.Input,
                SqlDbType = SqlDbType.VarBinary, // varbinary(128)
                ParameterName = "change_tracking_context",
                Value = ChangeTrackingContext.Identity.ToByteArray()
            });
        }
    }
}
