using System;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using System.Collections.Generic;

using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public partial class Entity
    {
        public sealed class DataMapper : IDataMapper
        {
            private const string SelectCommandText = @"SELECT [namespace], [owner], [parent], [name], [code], [version] FROM [entities] WHERE [key] = @key";
            private const string InsertCommandText =
                @"INSERT [entities] ([key], [namespace], [owner], [parent], [name], [code]) " +
                @"VALUES (@key, @namespace, @owner, @parent, @name, @code); " +
                @"IF @@ROWCOUNT > 0 SELECT [version] FROM [entities] WHERE [key] = @key;";
            private const string UpdateCommandText =
                @"DECLARE @rows_affected int; DECLARE @result table([version] binary(8)); " +
                @"UPDATE [entities] SET [namespace] = @namespace, [owner] = @owner, [parent] = @parent, " +
                @"[name] = @name, [code] = @code " +
                @"OUTPUT inserted.[version] INTO @result" +
                @" WHERE [key] = @key AND [version] = @version; " +
                @"SET @rows_affected = @@ROWCOUNT; " +
                @"IF (@rows_affected = 0) " +
                @"BEGIN " +
                @"  INSERT @result ([version]) SELECT [version] FROM [entities] WHERE [key] = @key; " +
                @"END " +
                @"SELECT @rows_affected, [version] FROM @result;";
            private const string DeleteCommandText =
                @"DELETE [entities] WHERE [key] = @key " +
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
                Entity e = (Entity)entity;

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
                        Guid guid;
                        e._namespace = Factory.New<Namespace>((Guid)reader[0]);
                        guid = (Guid)reader[1];
                        e.owner = (guid == Guid.Empty) ? null : Factory.New<Entity>(guid);
                        guid = (Guid)reader[2];
                        e.parent = (guid == Guid.Empty) ? null : Factory.New<Entity>(guid);
                        e.name = (string)reader[3];
                        e.code = (int)reader[4];
                        e.version = (byte[])reader[5];

                        ok = true;
                    }

                    reader.Close(); connection.Close();
                }

                if (!ok) throw new ApplicationException("Error executing select command.");
            }

            void IDataMapper.Insert(IPersistent entity)
            {
                Entity e = (Entity)entity;

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

                    parameter = new SqlParameter("namespace", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e._namespace == null) ? Guid.Empty : e._namespace.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("owner", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.owner == null) ? Guid.Empty : e.owner.identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("parent", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.parent == null) ? Guid.Empty : e.parent.identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("name", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.name == null) ? string.Empty : e.name;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("code", SqlDbType.Int);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.code;
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
                Entity e = (Entity)entity;

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

                    parameter = new SqlParameter("namespace", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e._namespace == null) ? Guid.Empty : e._namespace.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("owner", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.owner == null) ? Guid.Empty : e.owner.identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("parent", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.parent == null) ? Guid.Empty : e.parent.identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("name", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.name == null) ? string.Empty : e.name;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("code", SqlDbType.Int);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.code;
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
                Entity e = (Entity)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command  = connection.CreateCommand();
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