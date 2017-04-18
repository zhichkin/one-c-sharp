using System;
using System.Data;
using System.Data.SqlClient;

using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public partial class InfoBase
    {
        public sealed class DataMapper : IDataMapper
        {
            private const string SelectCommandText = @"SELECT [name], [server], [database], [username], [password], [version] FROM [metadata].[infobases] WHERE [key] = @key";
            private const string InsertCommandText =
                @"DECLARE @result table([version] binary(8)); " +
                @"INSERT [metadata].[infobases] ([key], [name], [server], [database], [username], [password]) " +
                @"OUTPUT inserted.[version] INTO @result " +
                @"VALUES (@key, @name, @server, @database, @username, @password); " +
                @"IF @@ROWCOUNT > 0 SELECT [version] FROM @result;";
            private const string UpdateCommandText =
                @"DECLARE @rows_affected int; DECLARE @result table([version] binary(8)); " +
                @"UPDATE [metadata].[infobases] SET [name] = @name, [server] = @server, [database] = @database, [username] = @username, [password] = @password " +
                @"OUTPUT inserted.[version] INTO @result" +
                @" WHERE [key] = @key AND [version] = @version; " +
                @"SET @rows_affected = @@ROWCOUNT; " +
                @"IF (@rows_affected = 0) " +
                @"BEGIN " +
                @"  INSERT @result ([version]) SELECT [version] FROM [metadata].[infobases] WHERE [key] = @key; " +
                @"END " +
                @"SELECT @rows_affected, [version] FROM @result;";
            private const string DeleteCommandText =
                @"DELETE [metadata].[infobases] WHERE [key] = @key " +
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
                InfoBase e = (InfoBase)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command  = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = SelectCommandText;

                    SqlParameter parameter = new SqlParameter("key", SqlDbType.UniqueIdentifier)
                    {
                        Direction = ParameterDirection.Input,
                        Value     = e.identity
                    };
                    command.Parameters.Add(parameter);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        e.name     = (string)reader[0];
                        e.server   = (string)reader[1];
                        e.database = (string)reader[2];
                        e.username = (string)reader[3];
                        e.password = (string)reader[4];
                        e.version  = (byte[])reader[5];
                        ok = true;
                    }

                    reader.Close(); connection.Close();
                }

                if (!ok) throw new ApplicationException("Error executing select command.");
            }

            void IDataMapper.Insert(IPersistent entity)
            {
                InfoBase e = (InfoBase)entity;

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

                    parameter = new SqlParameter("name", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.name;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("server", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.server;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("database", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.database;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("username", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.username;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("password", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.password;
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
                InfoBase e = (InfoBase)entity;

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

                    parameter = new SqlParameter("name", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.name;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("server", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.server;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("database", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.database;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("username", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.username;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("password", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.password;
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
                InfoBase e = (InfoBase)entity;

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

            public static Entity GetEntity(InfoBase infoBase, int typeCode)
            {
                Entity entity = null;

                IPersistentContext context = MetadataPersistentContext.Current;

                using (SqlConnection connection = new SqlConnection(context.ConnectionString))
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandType = CommandType.Text;

                    command.Parameters.AddWithValue("InfoBase", infoBase.identity);
                    command.Parameters.AddWithValue("TypeCode", typeCode);

                    command.CommandText =
                        @"WITH
                            namespaces ([owner], [key]) AS
                            (
	                            SELECT [owner], [key] FROM [metadata].[namespaces] WHERE [owner] = @InfoBase
	                            UNION ALL
	                            SELECT n.[owner], n.[key] FROM [metadata].[namespaces] AS n
	                            INNER JOIN
		                            namespaces AS anchor
	                            ON anchor.[key] = n.[owner]
                            )
                        SELECT
	                        e.[key]
                        FROM
	                        [metadata].[entities] AS e
	                        INNER JOIN namespaces AS n
	                        ON e.[namespace] = n.[key]
                        WHERE
	                        e.[code] = @TypeCode;";
                    
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            entity = context.Factory.New<Entity>(reader.GetGuid(0));
                        }
                    }
                }
                return entity;
            }
        }
    }
}
