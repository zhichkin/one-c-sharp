using System;
using System.Data;
using System.Data.SqlClient;
using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public partial class Request
    {
        public sealed class DataMapper : IDataMapper
        {
            #region " SQL "
            private const string SelectCommandText = @"SELECT [version], [name], [namespace], [owner], [parse_tree], [request_type], [response_type] FROM [metadata].[requests] WHERE [key] = @key";
            private const string InsertCommandText =
                @"DECLARE @result table([version] binary(8)); " +
                @"INSERT [metadata].[requests] ([key], [name], [namespace], [owner], [parse_tree], [request_type], [response_type]) " +
                @"OUTPUT inserted.[version] INTO @result " +
                @"VALUES (@key, @name, @namespace, @owner, @parse_tree, @request_type, @response_type); " +
                @"IF @@ROWCOUNT > 0 SELECT [version] FROM @result;";
            private const string UpdateCommandText =
                @"DECLARE @rows_affected int; DECLARE @result table([version] binary(8)); " +
                @"UPDATE [metadata].[requests] SET [name] = @name, [namespace] = @namespace, [owner] = @owner, [parse_tree] = @parse_tree, " +
                @"[request_type] = @request_type, [response_type] = @response_type " +
                @"OUTPUT inserted.[version] INTO @result" +
                @" WHERE [key] = @key AND [version] = @version; " +
                @"SET @rows_affected = @@ROWCOUNT; " +
                @"IF (@rows_affected = 0) " +
                @"BEGIN " +
                @"  INSERT @result ([version]) SELECT [version] FROM [metadata].[requests] WHERE [key] = @key; " +
                @"END " +
                @"SELECT @rows_affected, [version] FROM @result;";
            private const string DeleteCommandText =
                @"DELETE [metadata].[requests] WHERE [key] = @key " +
                @"   AND ([version] = @version OR @version = 0x00000000); " + // taking into account deletion of the entities having virtual state
                @"SELECT @@ROWCOUNT;";
            #endregion

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
                Request e = (Request)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = connection.CreateCommand();
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
                        e.version = (byte[])reader[0];

                        e.name = (string)reader[1];

                        Guid guid = (Guid)reader[2];
                        e._namespace = (guid == Guid.Empty) ? null : Factory.New<Namespace>(guid);

                        guid = (Guid)reader[3];
                        e.owner = (guid == Guid.Empty) ? null : Factory.New<Entity>(guid);

                        e.parseTree = (string)reader[4];

                        guid = (Guid)reader[5];
                        e.requestType = (guid == Guid.Empty) ? null : Factory.New<Entity>(guid);

                        guid = (Guid)reader[6];
                        e.responseType = (guid == Guid.Empty) ? null : Factory.New<Entity>(guid);

                        ok = true;
                    }

                    reader.Close(); connection.Close();
                }

                if (!ok) throw new ApplicationException("Error executing select command.");
            }
            void IDataMapper.Insert(IPersistent entity)
            {
                Request e = (Request)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = InsertCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("key", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("name", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.name == null) ? string.Empty : e.name;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("namespace", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e._namespace == null) ? Guid.Empty : e._namespace.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("owner", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.owner == null) ? Guid.Empty : e.owner.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("parse_tree", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.parseTree == null) ? string.Empty : e.parseTree;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("request_type", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.requestType == null) ? Guid.Empty : e.requestType.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("response_type", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.responseType == null) ? Guid.Empty : e.responseType.Identity;
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
                Request e = (Request)entity;

                bool ok = false; int rows_affected = 0;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = connection.CreateCommand();
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
                    parameter.Value = (e.name == null) ? string.Empty : e.name;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("namespace", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e._namespace == null) ? Guid.Empty : e._namespace.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("owner", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.owner == null) ? Guid.Empty : e.owner.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("parse_tree", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.parseTree == null) ? string.Empty : e.parseTree;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("request_type", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.requestType == null) ? Guid.Empty : e.requestType.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("response_type", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.responseType == null) ? Guid.Empty : e.responseType.Identity;
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
                Request e = (Request)entity;

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
        }
    }
}