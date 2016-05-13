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
    public partial class PropertyType
    {
        public sealed class DataMapper : IDataMapper
        {
            //private const string SelectCommandText = string.Empty;
            private const string InsertCommandText =
                @"INSERT [properties_types] ([property], [entity]) VALUES (@property, @entity); SELECT @@ROWCOUNT;";
            private const string UpdateCommandText =
                @"UPDATE [properties_types]" +
                @" SET [property] = @new_property, [entity] = @new_entity" +
                @" WHERE [property] = @old_property AND [entity] = @old_entity; " +
                @"SELECT @@ROWCOUNT;";
            private const string DeleteCommandText =
                @"DELETE [properties_types] WHERE [property] = @old_property AND [entity] = @old_entity; SELECT @@ROWCOUNT;";

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
                PropertyType e = (PropertyType)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command  = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = InsertCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("property", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.property.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("entity", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.type.Identity;
                    command.Parameters.Add(parameter);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read()) { ok = (int)reader[0] > 0; }

                    reader.Close(); connection.Close();
                }

                if (!ok) throw new ApplicationException("Error executing insert command.");
            }

            void IDataMapper.Update(IPersistent entity)
            {
                PropertyType e = (PropertyType)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command  = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = UpdateCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("new_property", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.property.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("new_entity", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.type.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("old_property", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.old_property;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("old_entity", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.old_type;
                    command.Parameters.Add(parameter);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read()) { ok = (int)reader[0] > 0; }

                    reader.Close(); connection.Close();
                }

                if (!ok) throw new ApplicationException("Error executing update command.");
            }

            void IDataMapper.Delete(IPersistent entity)
            {
                PropertyType e = (PropertyType)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = DeleteCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("old_property", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.old_property;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("old_entity", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.old_type;
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
