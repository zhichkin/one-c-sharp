using System;
using System.Data;
using System.Data.SqlClient;
using Zhichkin.ORM;
using System.Collections.Generic;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Integrator.Model
{
    public partial class Subscription
    {
        public sealed class DataMapper : IDataMapper
        {
            #region " SQL CRUD commands "
            private const string SelectCommandText =
                @"SELECT [version], [name], [last_sync_version], [msmq_target_queue] FROM [integrator].[publishers] WHERE [key] = @key";
            private const string InsertCommandText =
                @"DECLARE @result TABLE([version] binary(8)); " +
                @"INSERT [integrator].[publishers] ([key], [name], [last_sync_version], [msmq_target_queue]) " +
                @"OUTPUT inserted.[version] INTO @result " +
                @"VALUES (@key, @name, @last_sync_version, @msmq_target_queue); " +
                @"IF @@ROWCOUNT > 0 SELECT [version] FROM @result;";
            private const string UpdateCommandText =
                @"DECLARE @rows_affected int; DECLARE @result TABLE([version] binary(8)); " +
                @"UPDATE [integrator].[publishers] SET [name] = @name, [last_sync_version] = @last_sync_version, [msmq_target_queue] = @msmq_target_queue " +
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
                Subscription e = (Subscription)entity;

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
                            ok = true;
                        }
                    }
                }
                if (!ok) throw new ApplicationException("Error executing select command.");
            }
            void IDataMapper.Insert(IPersistent entity)
            {
                Subscription e = (Subscription)entity;

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
                Subscription e = (Subscription)entity;

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
                Subscription e = (Subscription)entity;

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

            public static IList<Subscription> Select()
            {
                IList<Subscription> list = new List<Subscription>();
                IPersistentContext context = IntegratorPersistentContext.Current;
                using (SqlConnection connection = new SqlConnection(context.ConnectionString))
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"SELECT [key], [version], [name], [last_sync_version], [msmq_target_queue] FROM [integrator].[publishers];";
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Subscription entity = (Subscription)context.Factory.New(typeof(Subscription), reader.GetGuid(0));
                            if (entity.State == PersistentState.New)
                            {
                                entity.State = PersistentState.Loading;
                                entity.version = (byte[])reader[1];
                                entity.name = reader.GetString(2);
                                entity.State = PersistentState.Original;
                            }
                            list.Add(entity);
                        }
                    }
                }
                return list;
            }
            public static IList<TranslationRule> GetTranslationRules(Subscription subscription)
            {
                IPersistentContext context = IntegratorPersistentContext.Current;
                List<TranslationRule> list = new List<TranslationRule>();

                string sql = @"SELECT {fields_list} FROM [{table_name}] WHERE [{fk_name}] = @key";

                using (SqlConnection connection = new SqlConnection(context.ConnectionString))
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    command.CommandType = CommandType.Text;
                    command.CommandText = sql;

                    SqlParameter parameter = new SqlParameter("key", System.Data.SqlDbType.UniqueIdentifier)
                    {
                        Direction = ParameterDirection.Input,
                        Value = subscription.Identity
                    };
                    command.Parameters.Add(parameter);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        TranslationRule rule = new TranslationRule();
                        rule.State = PersistentState.Loading;
                        rule.Source = context.Factory.New<Entity>(reader.GetGuid(0));
                        rule.Target = context.Factory.New<Entity>(reader.GetGuid(1));
                        rule.SourceProperty = context.Factory.New<Property>(reader.GetGuid(2));
                        rule.TargetProperty = context.Factory.New<Property>(reader.GetGuid(3));
                        rule.IsSyncKey = reader.GetBoolean(4);
                        rule.State = PersistentState.Original;
                        list.Add(rule);
                    }
                }
                return list;
            }
        }
    }
}
