﻿using System;
using System.Data;
using System.Data.SqlClient;

using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public partial class Entity
    {
        public sealed class DataMapper : IDataMapper
        {
            #region " SQL "
            private const string SelectCommandText = @"SELECT [namespace], [owner], [parent], [name], [code], [version], [alias], [is_abstract], [is_sealed] FROM [metadata].[entities] WHERE [key] = @key";
            private const string InsertCommandText =
                @"DECLARE @result table([version] binary(8)); " +
                @"INSERT [metadata].[entities] ([key], [namespace], [owner], [parent], [name], [code], [alias], [is_abstract], [is_sealed]) " +
                @"OUTPUT inserted.[version] INTO @result " +
                @"VALUES (@key, @namespace, @owner, @parent, @name, @code, @alias, @is_abstract, @is_sealed); " +
                @"IF @@ROWCOUNT > 0 SELECT [version] FROM @result;";
            private const string UpdateCommandText =
                @"DECLARE @rows_affected int; DECLARE @result table([version] binary(8)); " +
                @"UPDATE [metadata].[entities] SET [namespace] = @namespace, [owner] = @owner, [parent] = @parent, " +
                @"[name] = @name, [code] = @code, [alias] = @alias, [is_abstract] = @is_abstract, [is_sealed] = @is_sealed " +
                @"OUTPUT inserted.[version] INTO @result" +
                @" WHERE [key] = @key AND [version] = @version; " +
                @"SET @rows_affected = @@ROWCOUNT; " +
                @"IF (@rows_affected = 0) " +
                @"BEGIN " +
                @"  INSERT @result ([version]) SELECT [version] FROM [metadata].[entities] WHERE [key] = @key; " +
                @"END " +
                @"SELECT @rows_affected, [version] FROM @result;";
            private const string DeleteCommandText =
                @"DELETE [metadata].[entities] WHERE [key] = @key " +
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
                        e.alias = reader.GetString(6);
                        e.isAbstract = reader.GetBoolean(7);
                        e.isSealed = reader.GetBoolean(8);

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

                    parameter = new SqlParameter("alias", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.alias == null) ? string.Empty : e.alias;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("is_abstract", SqlDbType.Bit);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.isAbstract;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("is_sealed", SqlDbType.Bit);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.isSealed;
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

                    parameter = new SqlParameter("alias", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.alias == null) ? string.Empty : e.alias;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("is_abstract", SqlDbType.Bit);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.isAbstract;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("is_sealed", SqlDbType.Bit);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.isSealed;
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

            public static Entity Select(Guid identity)
            {
                Entity entity = null;
                IPersistentContext context = MetadataPersistentContext.Current;
                IReferenceObjectFactory factory = MetadataPersistentContext.Current.Factory;
                using (SqlConnection connection = new SqlConnection(context.ConnectionString))
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandType = CommandType.Text;
                    command.CommandText = SelectCommandText;
                    SqlParameter parameter = new SqlParameter("key", SqlDbType.UniqueIdentifier)
                    {
                        Direction = ParameterDirection.Input,
                        Value = identity
                    };
                    command.Parameters.Add(parameter);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            entity = (Entity)context.Factory.New(typeof(Entity), identity);
                            // check if nothing was found in IdentityMap - in-memory cash
                            if (entity.State == PersistentState.New)
                            {
                                entity.State = PersistentState.Loading;
                                Guid guid;
                                entity._namespace = factory.New<Namespace>((Guid)reader[0]);
                                guid = (Guid)reader[1];
                                entity.owner = (guid == Guid.Empty) ? null : factory.New<Entity>(guid);
                                guid = (Guid)reader[2];
                                entity.parent = (guid == Guid.Empty) ? null : factory.New<Entity>(guid);
                                entity.name = (string)reader[3];
                                entity.code = (int)reader[4];
                                entity.version = (byte[])reader[5];
                                entity.alias = reader.GetString(6);
                                entity.isAbstract = reader.GetBoolean(7);
                                entity.isSealed = reader.GetBoolean(8);
                                entity.State = PersistentState.Original;
                            }
                        }
                    }
                }
                return entity;
            }
        }
    }
}