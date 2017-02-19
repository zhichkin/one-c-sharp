using System;
using System.Data;
using System.Data.SqlClient;
using Zhichkin.ORM;
using System.Collections.Generic;

namespace Zhichkin.Integrator.Model
{
    public partial class Publisher
    {
        public sealed class DataMapper : IDataMapper
        {
            #region " SQL CRUD commands "
            private const string SelectCommandText =
                @"SELECT [version], [name], [last_sync_version], [msmq_target_queue], [change_tracking_system] FROM [integrator].[publishers] WHERE [key] = @key";
            private const string InsertCommandText =
                @"DECLARE @result TABLE([version] binary(8)); " +
                @"INSERT [integrator].[publishers] ([key], [name], [last_sync_version], [msmq_target_queue], [change_tracking_system]) " +
                @"OUTPUT inserted.[version] INTO @result " +
                @"VALUES (@key, @name, @last_sync_version, @msmq_target_queue, @change_tracking_system); " +
                @"IF @@ROWCOUNT > 0 SELECT [version] FROM @result;";
            private const string UpdateCommandText =
                @"DECLARE @rows_affected int; DECLARE @result TABLE([version] binary(8)); " +
                @"UPDATE [integrator].[publishers] SET [name] = @name, [last_sync_version] = @last_sync_version, [msmq_target_queue] = @msmq_target_queue, [change_tracking_system] = @change_tracking_system " +
                @"OUTPUT inserted.[version] INTO @result" +
                @" WHERE [key] = @key AND [version] = @version; " +
                @"SET @rows_affected = @@ROWCOUNT; " +
                @"IF (@rows_affected = 0) " +
                @"BEGIN " +
                @"  INSERT @result ([version]) SELECT [version] FROM [integrator].[publishers] WHERE [key] = @key; " +
                @"END " +
                @"SELECT @rows_affected, [version] FROM @result;";
            private const string DeleteCommandText =
                @"DELETE [integrator].[publishers] WHERE [key] = @key " +
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
                Publisher e = (Publisher)entity;

                bool ok = false;
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandType = CommandType.Text;
                    command.CommandText = SelectCommandText;
                    SqlParameter parameter = new SqlParameter("key", SqlDbType.UniqueIdentifier)
                    {
                        Direction = ParameterDirection.Input,
                        Value = e.identity
                    };
                    command.Parameters.Add(parameter);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            e.version = (byte[])reader[0];
                            e.name = reader.GetString(1);
                            e.last_sync_version = reader.GetInt64(2);
                            e.msmq_target_queue = reader.GetString(3);
                            e.change_tracking_system = (ChangeTrackingSystem)reader.GetInt32(4);
                            ok = true;
                        }
                    }
                }
                if (!ok) throw new ApplicationException("Error executing select command.");
            }
            void IDataMapper.Insert(IPersistent entity)
            {
                Publisher e = (Publisher)entity;

                bool ok = false;
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
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
                    parameter = new SqlParameter("last_sync_version", SqlDbType.BigInt);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.last_sync_version;
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter("msmq_target_queue", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.msmq_target_queue;
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter("change_tracking_system", SqlDbType.Int);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (int)e.change_tracking_system;
                    command.Parameters.Add(parameter);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            e.version = (byte[])reader[0]; ok = true;
                        }
                    }
                }
                if (!ok) throw new ApplicationException("Error executing insert command.");
            }
            void IDataMapper.Update(IPersistent entity)
            {
                Publisher e = (Publisher)entity;

                bool ok = false; int rows_affected = 0;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
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
                    parameter = new SqlParameter("last_sync_version", SqlDbType.BigInt);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.last_sync_version;
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter("msmq_target_queue", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.msmq_target_queue;
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter("change_tracking_system", SqlDbType.Int);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (int)e.change_tracking_system;
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
                Publisher e = (Publisher)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
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

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) { ok = reader.GetInt32(0) > 0; }
                    }
                }
                if (!ok) throw new ApplicationException("Error executing delete command.");
            }

            public static IList<Publisher> Select()
            {
                IList<Publisher> list = new List<Publisher>();
                IPersistentContext context = IntegratorPersistentContext.Current;
                using (SqlConnection connection = new SqlConnection(context.ConnectionString))
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"SELECT [key], [version], [name], [last_sync_version], [msmq_target_queue], [change_tracking_system] FROM [integrator].[publishers];";
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Publisher entity = (Publisher)context.Factory.New(typeof(Publisher), reader.GetGuid(0));
                            if (entity.State == PersistentState.New) // was not found in the cash (identity map)
                            {
                                entity.State = PersistentState.Loading;
                                entity.version = (byte[])reader[1];
                                entity.name = reader.GetString(2);
                                entity.last_sync_version = reader.GetInt64(3);
                                entity.msmq_target_queue = reader.GetString(4);
                                entity.change_tracking_system = (ChangeTrackingSystem)reader.GetInt32(5);
                                entity.State = PersistentState.Original;
                            }
                            list.Add(entity);
                        }
                    }
                }
                return list;
            }
            public static Publisher Select(Guid identity)
            {
                Publisher entity = null;
                IPersistentContext context = IntegratorPersistentContext.Current;
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
                            entity = (Publisher)context.Factory.New(typeof(Publisher), identity);
                            if (entity.State == PersistentState.New)
                            {
                                entity.State = PersistentState.Loading;
                                entity.version = (byte[])reader[0];
                                entity.name = reader.GetString(1);
                                entity.last_sync_version = reader.GetInt64(2);
                                entity.msmq_target_queue = reader.GetString(3);
                                entity.change_tracking_system = (ChangeTrackingSystem)reader.GetInt32(4);
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
