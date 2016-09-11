using System;
using System.Data;
using System.Data.SqlClient;
using Zhichkin.ORM;

namespace Zhichkin.Integrator.Model
{
    public partial class TranslationRule
    {
        public sealed class DataMapper : IDataMapper
        {
            //private const string SelectCommandText = string.Empty;
            private const string InsertCommandText =
                @"INSERT [relations] ([property], [entity]) VALUES (@property, @entity); SELECT @@ROWCOUNT;";
            private const string UpdateCommandText =
                @"UPDATE [relations]" +
                @" SET [property] = @new_property, [entity] = @new_entity" +
                @" WHERE [property] = @old_property AND [entity] = @old_entity; " +
                @"SELECT @@ROWCOUNT;";
            private const string DeleteCommandText =
                @"DELETE [relations] WHERE [property] = @old_property AND [entity] = @old_entity; SELECT @@ROWCOUNT;";

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
                TranslationRule e = (TranslationRule)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = InsertCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("source", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.source.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("target", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.target.Identity;
                    command.Parameters.Add(parameter);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read()) { ok = (int)reader[0] > 0; }

                    reader.Close(); connection.Close();
                }

                if (!ok) throw new ApplicationException("Error executing insert command.");
            }

            void IDataMapper.Update(IPersistent entity)
            {
                TranslationRule e = (TranslationRule)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = UpdateCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("new_source", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.source.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("new_target", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.target.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("old_source", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.source_old.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("old_target", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.target_old.Identity;
                    command.Parameters.Add(parameter);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read()) { ok = (int)reader[0] > 0; }

                    reader.Close(); connection.Close();
                }

                if (!ok) throw new ApplicationException("Error executing update command.");
            }

            void IDataMapper.Delete(IPersistent entity)
            {
                TranslationRule e = (TranslationRule)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = DeleteCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("old_source", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.source_old.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("old_target", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.target_old.Identity;
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
