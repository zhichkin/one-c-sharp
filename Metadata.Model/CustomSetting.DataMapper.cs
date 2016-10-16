using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public partial class CustomSetting
    {
        public sealed class DataMapper : IDataMapper
        {
            private const string SelectCommandText = @"SELECT [owner_], [owner], [name], [value], [version] FROM [metadata].[settings] WHERE [key] = @key";
            private const string InsertCommandText =
                @"DECLARE @result table([version] binary(8)); " +
                @"INSERT [metadata].[settings] ([key], [owner_], [owner], [name], [value]) " +
                @"OUTPUT inserted.[version] INTO @result " +
                @"VALUES (@key, @owner_, @owner, @name, @value); " +
                @"IF @@ROWCOUNT > 0 SELECT [version] FROM @result;";
            private const string UpdateCommandText =
                @"DECLARE @rows_affected int; DECLARE @result table([version] binary(8)); " +
                @"UPDATE [metadata].[settings]" +
                @" SET [owner_] = @owner_, [owner] = @owner, [name] = @name, [value] = @value " +
                @"OUTPUT inserted.[version] INTO @result" +
                @" WHERE [key] = @key AND [version] = @version; " +
                @"SET @rows_affected = @@ROWCOUNT; " +
                @"IF (@rows_affected = 0) " +
                @"BEGIN " +
                @"  INSERT @result ([version]) SELECT [version] FROM [metadata].[settings] WHERE [key] = @key; " +
                @"END " +
                @"SELECT @rows_affected, [version] FROM @result;";
            private const string DeleteCommandText =
                @"DELETE [metadata].[settings] WHERE [key] = @key " +
                @"   AND ([version] = @version OR @version = 0x00000000); " + // taking into account deletion of the entities having virtual state
                @"SELECT @@ROWCOUNT;";

            private readonly string ConnectionString;
            private readonly IReferenceObjectFactory Factory;
            private DataMapper() { }
            public DataMapper(string connectionString, IReferenceObjectFactory factory)
            {
                ConnectionString = connectionString;
                Factory = factory;
            }
            void IDataMapper.Select(IPersistent entity)
            {
                CustomSetting e = (CustomSetting)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command  = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = SelectCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("key", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.identity;
                    command.Parameters.Add(parameter);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        e.owner = (EntityBase)Factory.New((int)reader[0], (Guid)reader[1]);
                        e.name = (string)reader[2];
                        e._value = (string)reader[3];
                        e.version = (byte[])reader[4];

                        ok = true;
                    }

                    reader.Close(); connection.Close();
                }

                if (!ok) throw new ApplicationException("Error executing select command.");
            }
            void IDataMapper.Insert(IPersistent entity)
            {
                CustomSetting e = (CustomSetting)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command  = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = InsertCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("key", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("owner_", SqlDbType.Int);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.owner == null) ? 0 : e.owner.TypeCode;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("owner", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.owner == null) ? Guid.Empty : e.owner.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("name", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.name == null) ? string.Empty : e.name;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("value", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e._value == null) ? string.Empty : e._value;
                    command.Parameters.Add(parameter);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        e.version = (byte[])reader[0]; ok = true;
                    }

                    reader.Close(); connection.Close();
                }

                if (!ok) throw new ApplicationException("Error executing insert command.");
            }
            void IDataMapper.Update(IPersistent entity)
            {
                CustomSetting e = (CustomSetting)entity;

                bool ok = false; int rows_affected = 0;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command  = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = UpdateCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("key", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("version", SqlDbType.Timestamp);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.version;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("owner_", SqlDbType.Int);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.owner == null) ? 0 : e.owner.TypeCode;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("owner", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.owner == null) ? Guid.Empty : e.owner.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("name", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.name == null) ? string.Empty : e.name;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("value", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e._value == null) ? string.Empty : e._value;
                    command.Parameters.Add(parameter);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            rows_affected = reader.GetInt32(0);
                            e.version = (byte[])reader[1];
                            if (rows_affected == 0)
                            {
                                e.state = PersistentState.Changed;
                            }
                            else
                            {
                                ok = true;
                            }
                        }
                        else
                        {
                            e.state = PersistentState.Deleted;
                        }
                    }
                }

                if (!ok) throw new OptimisticConcurrencyException(e.state.ToString());
            }
            void IDataMapper.Delete(IPersistent entity)
            {
                CustomSetting e = (CustomSetting)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = DeleteCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("key", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("version", SqlDbType.Timestamp);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.version;
                    command.Parameters.Add(parameter);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read()) { ok = (int)reader[0] > 0; }

                    reader.Close(); connection.Close();
                }

                if (!ok) throw new ApplicationException("Error executing delete command.");
            }

            public static IDictionary<string, CustomSetting> Select(EntityBase owner)
            {
                IPersistentContext context = MetadataPersistentContext.Current;

                Dictionary<string, CustomSetting> settings = new Dictionary<string, CustomSetting>();

                string sql = @"SELECT [key], [owner_], [owner], [name], [value], [version] FROM [metadata].[settings] WHERE [owner_] = @code AND [owner] = @owner";

                using (SqlConnection connection = new SqlConnection(context.ConnectionString))
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    command.CommandType = CommandType.Text;
                    command.CommandText = sql;

                    SqlParameter parameter = new SqlParameter("code", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Input,
                        Value = (owner == null) ? 0 : owner.TypeCode
                    };
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("owner", SqlDbType.UniqueIdentifier)
                    {
                        Direction = ParameterDirection.Input,
                        Value = (owner == null) ? Guid.Empty : owner.Identity
                    };
                    command.Parameters.Add(parameter);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        CustomSetting setting = (CustomSetting)context.Factory.New(typeof(CustomSetting), reader.GetGuid(0));
                        if (setting.State == PersistentState.New) // was not found in the context cash (identity map)
                        {
                            setting.State = PersistentState.Loading;
                            setting.owner = (EntityBase)context.Factory.New(reader.GetInt32(1), reader.GetGuid(2));
                            setting.name = reader.GetString(3);
                            setting._value = reader.GetString(4);
                            setting.version = (byte[])reader[5];
                            setting.State = PersistentState.Original;
                        }
                        settings.Add(setting.name, setting);
                    }
                }

                return settings;
            }
        }
    }
}
