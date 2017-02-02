using System;
using System.Data;
using System.Data.SqlClient;
using Zhichkin.ORM;
using Zhichkin.Metadata.Model;
using System.Collections.Generic;

namespace Zhichkin.Integrator.Model
{
    public partial class AggregateItem
    {
        public sealed class DataMapper : IDataMapper
        {
            //private const string SelectCommandText = string.Empty;
            private const string InsertCommandText =
                @"INSERT [integrator].[aggregates] ([aggregate], [component], [connector]) VALUES (@aggregate, @component, @connector); SELECT @@ROWCOUNT;";
            private const string UpdateCommandText =
                @"UPDATE [integrator].[aggregates]" +
                @" SET [aggregate] = @new_aggregate, [component] = @new_component, [connector] = @new_connector " +
                @" WHERE [aggregate] = @old_aggregate AND [component] = @old_component; " +
                @"SELECT @@ROWCOUNT;";
            private const string DeleteCommandText =
                @"DELETE [integrator].[aggregates] WHERE [aggregate] = @old_aggregate AND [component] = @old_component; SELECT @@ROWCOUNT;";

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
                throw new NotSupportedException();
            }
            void IDataMapper.Insert(IPersistent entity)
            {
                AggregateItem e = (AggregateItem)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command  = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = InsertCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("aggregate", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.aggregate.Identity;
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter("component", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.component.Identity;
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter("connector", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.connector.Identity;
                    command.Parameters.Add(parameter);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read()) { ok = (int)reader[0] > 0; }

                    reader.Close(); connection.Close();
                }

                if (!ok) throw new ApplicationException("Error executing insert command.");
            }
            void IDataMapper.Update(IPersistent entity)
            {
                AggregateItem e = (AggregateItem)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = UpdateCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("new_aggregate", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.aggregate.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("old_aggregate", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.aggregate_old.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("new_component", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.component.Identity;
                    command.Parameters.Add(parameter);
                    
                    parameter = new SqlParameter("old_component", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.component_old.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("new_connector", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.connector.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("old_connector", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.connector_old.Identity;
                    command.Parameters.Add(parameter);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read()) { ok = (int)reader[0] > 0; }

                    reader.Close(); connection.Close();
                }

                if (!ok) throw new ApplicationException("Error executing update command.");
            }
            void IDataMapper.Delete(IPersistent entity)
            {
                AggregateItem e = (AggregateItem)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = DeleteCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("old_aggregate", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.aggregate_old.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("old_component", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.component_old.Identity;
                    command.Parameters.Add(parameter);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read()) { ok = (int)reader[0] > 0; }

                    reader.Close(); connection.Close();
                }

                if (!ok) throw new ApplicationException("Error executing delete command.");
            }

            public static IList<AggregateItem> Select(Entity aggregate)
            {
                IList<AggregateItem> list = new List<AggregateItem>();
                IPersistentContext context = IntegratorPersistentContext.Current;
                using (SqlConnection connection = new SqlConnection(context.ConnectionString))
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"SELECT [aggregate], [component], [connector] FROM [integrator].[aggregates] WHERE [aggregate] = @aggregate;";
                    SqlParameter parameter = new SqlParameter("aggregate", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = aggregate.Identity;
                    command.Parameters.Add(parameter);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AggregateItem item = new AggregateItem();
                            item.State = PersistentState.Loading;
                            item.aggregate = context.Factory.New<Entity>(reader.GetGuid(0));
                            item.component = context.Factory.New<Entity>(reader.GetGuid(1));
                            item.connector = context.Factory.New<Property>(reader.GetGuid(2));
                            item.State = PersistentState.Original;
                            list.Add(item);
                        }
                    }
                }
                return list;
            }
            public static AggregateItem Select(Entity aggregate, Entity component)
            {
                AggregateItem item = null;
                IPersistentContext context = IntegratorPersistentContext.Current;
                using (SqlConnection connection = new SqlConnection(context.ConnectionString))
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"SELECT [aggregate], [component], [connector] FROM [integrator].[aggregates] WHERE [aggregate] = @aggregate AND [component] = @component;";
                    SqlParameter parameter = new SqlParameter("aggregate", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = aggregate.Identity;
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter("component", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = component.Identity;
                    command.Parameters.Add(parameter);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            item = new AggregateItem();
                            item.State = PersistentState.Loading;
                            item.aggregate = context.Factory.New<Entity>(reader.GetGuid(0));
                            item.component = context.Factory.New<Entity>(reader.GetGuid(1));
                            item.connector = context.Factory.New<Property>(reader.GetGuid(2));
                            item.State = PersistentState.Original;
                        }
                    }
                }
                return item;
            }
            public static IList<Property> GetConnectors(Entity aggregate, Entity component)
            {
                IList<Property> list = new List<Property>();
                IPersistentContext context = IntegratorPersistentContext.Current;
                using (SqlConnection connection = new SqlConnection(context.ConnectionString))
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"
SELECT
	r.[property]
FROM
	[metadata].[relations] AS r
	INNER JOIN
		[metadata].[properties] AS p
	ON r.[property] = p.[key]
WHERE
	r.[entity] = @aggregate
	AND
	p.[entity] = @component";
                    SqlParameter parameter = new SqlParameter("aggregate", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = aggregate.Identity;
                    command.Parameters.Add(parameter);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Property item = new Property();
                            //item.State = PersistentState.Loading;
                            //item.aggregate = context.Factory.New<Entity>(reader.GetGuid(0));
                            //item.component = context.Factory.New<Entity>(reader.GetGuid(1));
                            //item.connector = context.Factory.New<Property>(reader.GetGuid(2));
                            //item.State = PersistentState.Original;
                            list.Add(item);
                        }
                    }
                }
                return list;
            }
        }
    }
}
