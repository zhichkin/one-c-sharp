using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Metadata.Services
{
    public sealed class SQLBuilder
    {
        private IMetadataService service = new MetadataService();
        private string _ConnectionString = MetadataPersistentContext.Current.ConnectionString;

        public bool SchemaExists(string schema_name)
        {
            int result = 0;

            StringBuilder script = new StringBuilder();
            script.Append("SELECT COUNT(*) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = @schema_name;");

            using (SqlConnection connection = new SqlConnection(_ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = script.ToString();

                command.Parameters.AddWithValue("schema_name", schema_name);

                result = (int)command.ExecuteScalar();
            }

            return (result == 1);
        }
        public bool TableExists(Table table)
        {
            int result = 0;

            StringBuilder script = new StringBuilder();
            script.Append("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @schema_name AND TABLE_NAME = @table_name;");

            using (SqlConnection connection = new SqlConnection(_ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = script.ToString();

                command.Parameters.AddWithValue("schema_name", table.Schema);
                command.Parameters.AddWithValue("table_name",  table.Name);

                result = (int)command.ExecuteScalar();
            }

            return (result == 1);
        }
        public void CreateSchema(string schema_name)
        {
            using (SqlConnection connection = new SqlConnection(_ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $"CREATE SCHEMA [{schema_name}];";
                int result = (int)command.ExecuteNonQuery();
            }
        }
        public void DropSchema(string schema_name)
        {
            using (SqlConnection connection = new SqlConnection(_ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $"DROP SCHEMA [{schema_name}];";
                int result = (int)command.ExecuteNonQuery();
            }
        }
        public void DropTable(Table table)
        {
            if (!TableExists(table)) return;

            string schema_name = string.IsNullOrWhiteSpace(table.Schema) ? "dbo" : table.Schema;

            using (SqlConnection connection = new SqlConnection(_ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $"DROP TABLE [{schema_name}].[{table.Name}];";
                int result = (int)command.ExecuteNonQuery();
            }
        }
        public void CreateTable(Table table)
        {
            if (TableExists(table)) return;

            string schema_name = string.IsNullOrWhiteSpace(table.Schema) ? "dbo" : table.Schema;
            if (!SchemaExists(schema_name)) CreateSchema(schema_name);

            StringBuilder sql = new StringBuilder();
            sql.AppendLine($"CREATE TABLE [{schema_name}].[{table.Name}]");
            sql.AppendLine("(");
            foreach (Field field in table.Fields)
            {
                sql.AppendLine($"\t{CreateTableFieldScript(field)},");
            }
            sql.AppendLine($"\t{CreatePrimaryKeyConstraintScript(table)}");
            sql.Append(");");

            using (SqlConnection connection = new SqlConnection(_ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = sql.ToString();
                int result = (int)command.ExecuteNonQuery();
            }
        }
        private string CreateTableFieldScript(Field field)
        {
            string SIZE = string.Empty;
            if (field.TypeName == "char"
                | field.TypeName == "nchar"
                | field.TypeName == "binary"
                | field.TypeName == "varchar"
                | field.TypeName == "nvarchar"
                | field.TypeName == "varbinary")
            {
                SIZE = (field.Length < 0) ? "(MAX)" : $"({field.Length})";
            }
            else if (field.Precision > 0 && field.TypeName != "bit")
            {
                SIZE = $"({field.Precision}, {field.Scale})";
            }
            string NULLABLE = field.IsNullable ? " NULL" : " NOT NULL";
            return $"[{field.Name}] [{field.TypeName}]{SIZE}{NULLABLE}";
        }
        private string CreatePrimaryKeyConstraintScript(Table table)
        {
            StringBuilder script = new StringBuilder();
            foreach (Field field in table.Fields
                .Where(f => f.IsPrimaryKey)
                .OrderBy(f => f.KeyOrdinal))
            {
                if (script.Length > 0) script.Append(", ");
                script.Append($"[{field.Name}]");
            }
            return $"CONSTRAINT [pk_{table.Name}] PRIMARY KEY CLUSTERED ({script.ToString()})";
        }


    }
}

//CREATE TABLE [metadata].[namespaces]
//(
//   [key][uniqueidentifier] NOT NULL,
//   [version] [timestamp] NOT NULL,
//   [owner] [uniqueidentifier] NOT NULL,
//   [owner_] [int] NOT NULL,
//   [name] [nvarchar] (100) NOT NULL,
//   CONSTRAINT[PK_namespaces] PRIMARY KEY CLUSTERED([key])
//);
