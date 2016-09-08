using System;
using System.Text;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using Zhichkin.Metadata.Model;
using System.Collections.Generic;

namespace Zhichkin.ChangeTracking
{
    public sealed class ChangeTrackingService : IChangeTrackingService
    {
        private readonly string connectionString;
        public ChangeTrackingService(string connectionString)
        {
            this.connectionString = connectionString;
        }

        private string GetFullTableName(Table table)
        {
            string tableName = string.Empty;
            if (string.IsNullOrWhiteSpace(table.Schema))
            {
                tableName = string.Format("[{0}]", table.Name);
            }
            else
            {
                tableName = string.Format("[{0}].[{1}]", table.Schema, table.Name);
            }
            return tableName;
        }
        private string ClusteredIndexInfoScript
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"SELECT");
                sb.AppendLine(@"    i.name,");
                sb.AppendLine(@"    i.is_unique,");
                sb.AppendLine(@"    i.is_primary_key,");
                sb.AppendLine(@"    c.key_ordinal,");
                sb.AppendLine(@"    f.name,");
                sb.AppendLine(@"    f.is_nullable");
                sb.AppendLine(@"FROM sys.indexes AS i");
                sb.AppendLine(@"INNER JOIN sys.tables AS t ON t.object_id = i.object_id");
                sb.AppendLine(@"INNER JOIN sys.index_columns AS c ON c.object_id = t.object_id AND c.index_id = i.index_id");
                sb.AppendLine(@"INNER JOIN sys.columns AS f ON f.object_id = t.object_id AND f.column_id = c.column_id");
                sb.AppendLine(@"WHERE");
                sb.AppendLine(@"    t.object_id = OBJECT_ID(@table) AND i.type = 1 -- CLUSTERED");
                sb.AppendLine(@"ORDER BY");
                sb.AppendLine(@"c.key_ordinal ASC;");
                return sb.ToString();
            }
        }
        public ClusteredIndexInfo GetClusteredIndexInfo(Table table)
        {
            ClusteredIndexInfo info = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(ClusteredIndexInfoScript, connection))
            {
                connection.Open();

                command.Parameters.AddWithValue("table", GetFullTableName(table));
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        info = new ClusteredIndexInfo()
                        {
                            NAME           = reader.GetString(0),
                            IS_UNIQUE      = reader.GetBoolean(1),
                            IS_PRIMARY_KEY = reader.GetBoolean(2)
                        };
                        info.COLUMNS.Add(new ClusteredIndexColumnInfo()
                        {
                            KEY_ORDINAL = reader.GetByte(3),
                            NAME        = reader.GetString(4),
                            IS_NULLABLE = reader.GetBoolean(5)
                        });
                        while (reader.Read())
                        {
                            info.COLUMNS.Add(new ClusteredIndexColumnInfo()
                            {
                                KEY_ORDINAL = reader.GetByte(3),
                                NAME        = reader.GetString(4),
                                IS_NULLABLE = reader.GetBoolean(5)
                            });
                        }
                    }
                }
            }            
            return info;
        }
        public ClusteredIndexInfo GetClusteredIndexInfo(Table table, SqlCommand command)
        {
            ClusteredIndexInfo info = null;
            command.CommandType = CommandType.Text;
            command.CommandText = ClusteredIndexInfoScript;
            command.Parameters.Clear();
            command.Parameters.AddWithValue("table", GetFullTableName(table));
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    info = new ClusteredIndexInfo()
                    {
                        NAME           = reader.GetString(0),
                        IS_UNIQUE      = reader.GetBoolean(1),
                        IS_PRIMARY_KEY = reader.GetBoolean(2)
                    };
                    info.COLUMNS.Add(new ClusteredIndexColumnInfo()
                    {
                        KEY_ORDINAL = reader.GetByte(3),
                        NAME        = reader.GetString(4),
                        IS_NULLABLE = reader.GetBoolean(5)
                    });
                }
                while (reader.Read())
                {
                    info.COLUMNS.Add(new ClusteredIndexColumnInfo()
                    {
                        KEY_ORDINAL = reader.GetByte(3),
                        NAME        = reader.GetString(4),
                        IS_NULLABLE = reader.GetBoolean(5)
                    });
                }
            }
            return info;
        }

        private string SnapshotIsolationStateScript
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"SELECT");
                sb.AppendLine(@"    snapshot_isolation_state,");
                sb.AppendLine(@"    is_read_committed_snapshot_on");
                sb.AppendLine(@"FROM");
                sb.AppendLine(@"    sys.databases");
                sb.AppendLine(@"WHERE");
                sb.AppendLine(@"    name = @database;");
                return sb.ToString();
            }
        }
        private string SwitchSnapshotIsolationStateTemplate
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"ALTER DATABASE {0} SET ALLOW_SNAPSHOT_ISOLATION {1};");
                return sb.ToString();
            }
        }
        public SnapshotIsolationState GetSnapshotIsolationState(InfoBase infoBase)
        {
            SnapshotIsolationState state = SnapshotIsolationState.OFF;
            string sql = SnapshotIsolationStateScript;
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("database", infoBase.Database);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        state = (SnapshotIsolationState)reader.GetByte(0);
                    }
                }
            }
            return state;
        }
        public void SwitchSnapshotIsolationState(InfoBase infoBase, bool on)
        {
            string sql = SwitchSnapshotIsolationStateTemplate;
            sql = string.Format(sql, infoBase.Database,
                on ?
                SnapshotIsolationState.ON.ToString() :
                SnapshotIsolationState.OFF.ToString());
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private string ChangeTrackingDatabaseInfoScript
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"SELECT");
                sb.AppendLine(@"    c.is_auto_cleanup_on,");
                sb.AppendLine(@"    c.retention_period,");
                sb.AppendLine(@"    c.retention_period_units,");
                sb.AppendLine(@"    c.retention_period_units_desc,");
                sb.AppendLine(@"    c.max_cleanup_version");
                sb.AppendLine(@"FROM");
                sb.AppendLine(@"    sys.change_tracking_databases AS c");
                sb.AppendLine(@"INNER JOIN");
                sb.AppendLine(@"    sys.databases AS d ON c.database_id = d.database_id");
                sb.AppendLine(@"WHERE");
                sb.AppendLine(@"    d.name = @database;");
                return sb.ToString();
            }
        }
        private string ChangeTrackingTableInfoScript
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"SELECT");
                sb.AppendLine(@"    is_track_columns_updated_on,");
                sb.AppendLine(@"    begin_version,");
                sb.AppendLine(@"    min_valid_version,");
                sb.AppendLine(@"    cleanup_version");
                sb.AppendLine(@"FROM");
                sb.AppendLine(@"    sys.change_tracking_tables");
                sb.AppendLine(@"WHERE");
                sb.AppendLine(@"    object_id = OBJECT_ID(@table);");
                return sb.ToString();
            }
        }
        public ChangeTrackingDatabaseInfo GetChangeTrackingDatabaseInfo(InfoBase infoBase)
        {
            ChangeTrackingDatabaseInfo info = null;

            string sql = ChangeTrackingDatabaseInfoScript;

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("database", infoBase.Database);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        info = new ChangeTrackingDatabaseInfo()
                        {
                            IS_AUTO_CLEANUP_ON          = reader.GetByte(0) == 0 ? false : true,
                            RETENTION_PERIOD            = reader.GetInt32(1),
                            RETENTION_PERIOD_UNITS      = reader.GetByte(2),
                            RETENTION_PERIOD_UNITS_DESC = reader.GetString(3),
                            MAX_CLEANUP_VERSION         = (reader.IsDBNull(4) ? (long)0 : reader.GetInt64(4))
                        };
                    }
                }
            }
            return info;
        }
        public ChangeTrackingTableInfo GetChangeTrackingTableInfo(Table table)
        {
            ChangeTrackingTableInfo info = null;

            string sql = ChangeTrackingTableInfoScript;

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("table", GetFullTableName(table));

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        info = new ChangeTrackingTableInfo()
                        {
                            IS_TRACK_COLUMNS_UPDATED_ON = reader.GetBoolean(0),
                            BEGIN_VERSION               = reader.GetInt64(1),
                            MIN_VALID_VERSION           = reader.GetInt64(2),
                            CLEANUP_VERSION             = reader.IsDBNull(3) ? 0 : reader.GetInt64(3)
                        };
                    }
                }
            }
            return info;
        }

        private string EnableDatabaseChangeTrackingTemplate
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"ALTER DATABASE [{0}] SET CHANGE_TRACKING = ON(CHANGE_RETENTION = {1} {2}, AUTO_CLEANUP = {3});");
                return sb.ToString();
            }
        }
        private string AlterDatabaseChangeTrackingTemplate
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"ALTER DATABASE [{0}] SET CHANGE_TRACKING (CHANGE_RETENTION = {1} {2}, AUTO_CLEANUP = {3});");
                return sb.ToString();
            }
        }
        private string DisableDatabaseChangeTrackingTemplate
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"ALTER DATABASE [{0}] SET CHANGE_TRACKING = OFF;");
                return sb.ToString();
            }
        }
        public void EnableDatabaseChangeTracking(InfoBase infoBase, ChangeTrackingDatabaseInfo info)
        {
            string sql = (info == null) ?
                EnableDatabaseChangeTrackingTemplate :
                AlterDatabaseChangeTrackingTemplate;

            if (info == null)
            {
                info = new ChangeTrackingDatabaseInfo()
                {
                    IS_AUTO_CLEANUP_ON          = true,
                    RETENTION_PERIOD            = 2,
                    RETENTION_PERIOD_UNITS      = 3,
                    RETENTION_PERIOD_UNITS_DESC = "DAYS"
                };
            }

            sql = string.Format(sql,
                infoBase.Database,
                info.RETENTION_PERIOD,
                (info.RETENTION_PERIOD_UNITS == 1 ? "MINUTES" : (info.RETENTION_PERIOD_UNITS == 2 ? "HOURS" : "DAYS")),
                (info.IS_AUTO_CLEANUP_ON ? "ON" : "OFF"));

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        public void DisableDatabaseChangeTracking(InfoBase infoBase)
        {
            string sql = DisableDatabaseChangeTrackingTemplate;

            sql = string.Format(sql, infoBase.Database);

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private string EnableTableChangeTrackingTemplate
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"ALTER TABLE {0} ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = OFF);");
                return sb.ToString();
            }
        }
        private string EnableChangeTrackingClusteredIndexTemplate
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"BEGIN TRY");
                sb.AppendLine(@"    BEGIN TRANSACTION;");
                sb.AppendLine(@"    IF EXISTS(SELECT * FROM sys.indexes WHERE[object_id] = OBJECT_ID('{0}') AND [name] = '{1}')");
                sb.AppendLine(@"    BEGIN");
                sb.AppendLine(@"        DROP INDEX [{1}] ON {0};");
                sb.AppendLine(@"    END");
                sb.AppendLine(@"    ALTER TABLE {0} ADD CONSTRAINT [{1}] PRIMARY KEY CLUSTERED({2});");
                sb.AppendLine(@"    ALTER TABLE {0} ENABLE CHANGE_TRACKING WITH(TRACK_COLUMNS_UPDATED = OFF);");
                sb.AppendLine(@"    COMMIT TRANSACTION;");
                sb.AppendLine(@"END TRY");
                sb.AppendLine(@"BEGIN CATCH");
                sb.AppendLine(@"    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;");
                sb.AppendLine(@"END CATCH;");
                return sb.ToString();
            }
        }
        private string DisableTableChangeTrackingTemplate
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"ALTER TABLE {0} DISABLE CHANGE_TRACKING;");
                return sb.ToString();
            }
        }
        private string SwitchTableColumnsTrackingScript
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"ALTER TABLE {0} DISABLE CHANGE_TRACKING;");
                sb.AppendLine(@"ALTER TABLE {0} ENABLE CHANGE_TRACKING WITH(TRACK_COLUMNS_UPDATED = {1});");
                return sb.ToString();
            }
        }
        public void SwitchTableChangeTracking(Table table, bool on)
        {
            if (!on)
            {
                DisableTableChangeTracking(table);
                return;
            }

            ClusteredIndexInfo info = GetClusteredIndexInfo(table);
            if (info == null) throw new NotSupportedException("Clustered index not found!");
            if (info.HasNullableColumns) throw new NotSupportedException("Primary key can not have nullable columns!");

            if (info.IS_PRIMARY_KEY)
            {
                EnableTableChangeTracking(table);
            }
            else
            {
                EnableTableChangeTracking(table, info);
            }
        }
        private void EnableTableChangeTracking(Table table)
        {
            string sql = EnableTableChangeTrackingTemplate;
            sql = string.Format(sql, GetFullTableName(table));
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        private void EnableTableChangeTracking(Table table, ClusteredIndexInfo info)
        {
            string sql = EnableChangeTrackingClusteredIndexTemplate;
            string columns = string.Empty;
            foreach (ClusteredIndexColumnInfo column in info.COLUMNS)
            {
                columns += (columns == string.Empty ? string.Empty : ", ") + string.Format("[{0}]", column.NAME);
            }
            sql = string.Format(sql, GetFullTableName(table), info.NAME, columns);
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        private void DisableTableChangeTracking(Table table)
        {
            string sql = DisableTableChangeTrackingTemplate;
            sql = string.Format(sql, GetFullTableName(table));
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
        public void SwitchTableColumnsTracking(Table table, bool on)
        {
            string sql = SwitchTableColumnsTrackingScript;
            sql = string.Format(sql, GetFullTableName(table), on ? "ON" : "OFF");
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private string GetMinValidSyncVersionScript
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"SELECT CHANGE_TRACKING_MIN_VALID_VERSION(OBJECT_ID(@table));");
                return sb.ToString();
            }
        }
        private string GetCurrentSyncVersionScript
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"SELECT CHANGE_TRACKING_CURRENT_VERSION();");
                return sb.ToString();
            }
        }
        private string SelectChangesTemplate
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"SELECT");
                sb.AppendLine(@"    c.SYS_CHANGE_OPERATION,");
                sb.AppendLine(@"    {2}");
                sb.AppendLine(@"FROM");
                sb.AppendLine(@"    CHANGETABLE(CHANGES {0}, @last_sync_version) AS c");
                sb.AppendLine(@"    LEFT JOIN {0} AS d");
                sb.AppendLine(@"    ON {1}");
                sb.AppendLine(@"WHERE");
                sb.AppendLine(@"    c.SYS_CHANGE_CONTEXT IS NULL");
                sb.AppendLine(@"    OR");
                sb.AppendLine(@"    c.SYS_CHANGE_CONTEXT <> CAST(@target AS varbinary(128));");
                return sb.ToString();
            }
        }
        private string GetSelectChangesScript(Table table, SqlCommand command)
        {
            string sql = SelectChangesTemplate;
            string tableName = GetFullTableName(table);
            string keys = GetPrimaryKeysJoinScript(table, command);
            string fields = GetSelectFieldsScript(table);
            string.Format(sql, tableName, keys, fields);
            return sql;
        }
        private string GetPrimaryKeysJoinScript(Table table, SqlCommand command)
        {
            ClusteredIndexInfo index = GetClusteredIndexInfo(table, command);
            if (index == null) throw new InvalidOperationException();

            string sql = string.Empty;
            foreach (ClusteredIndexColumnInfo column in index.COLUMNS)
            {
                sql += (sql == string.Empty ? string.Empty : " AND ");
                sql += string.Format("c.[{0}] = d.[{0}]", column.NAME);
            }
            return sql;
        }
        private string GetSelectFieldsScript(Table table)
        {
            string sql = string.Empty;
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                foreach (Field field in table.Fields)
                {
                    sql += (sql == string.Empty ? string.Empty : ", ");
                    sql += string.Format("{0}.[{1}]", (field.IsPrimaryKey ? "c" : "d"), field.Name);
                }
                scope.Complete();
            }
            return sql;
        }
        public long GetMinValidSyncVersion(Table table, SqlCommand command)
        {
            command.CommandType = CommandType.Text;
            command.CommandText = GetMinValidSyncVersionScript;
            command.Parameters.Clear();
            command.Parameters.AddWithValue("table", GetFullTableName(table));
            object result = command.ExecuteScalar();
            if (result == DBNull.Value) return 0;
            return (long)result;
        }
        public long GetCurrentSyncVersion(SqlCommand command)
        {
            command.CommandType = CommandType.Text;
            command.CommandText = GetCurrentSyncVersionScript;
            command.Parameters.Clear();
            object result = command.ExecuteScalar();
            if (result == DBNull.Value) return 0;
            return (long)result;
        }
        public List<ChangeTrackingRecord> SelectChanges(Table table, long last_sync_version, SqlCommand command)
        {
            command.CommandType = CommandType.Text;
            command.CommandText = GetSelectChangesScript(table, command);
            command.Parameters.Clear();
            command.Parameters.AddWithValue("target", Guid.Empty);
            command.Parameters.AddWithValue("last_sync_version", last_sync_version);

            List<ChangeTrackingRecord> changes = new List<ChangeTrackingRecord>();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    ChangeTrackingRecord record = new ChangeTrackingRecord();
                    // 0 - SYS_CHANGE_OPERATION nchar(1) I, U, D
                    record.SYS_CHANGE_OPERATION = reader.GetString(0);
                    List<ChangeTrackingField> list = new List<ChangeTrackingField>();
                    for (int i = 1; i < reader.FieldCount; i++)
                    {
                        ChangeTrackingField field = new ChangeTrackingField();
                        field.Name = reader.GetName(i);
                        field.Value = reader.IsDBNull(i) ? null : reader[i];
                        field.IsKey = table.Fields.Where(f => f.Name == field.Name).FirstOrDefault().IsPrimaryKey;
                        list.Add(field);
                    }
                    record.Fields = list.ToArray();
                    changes.Add(record);
                }
            }
            return changes;
        }
    }
}
