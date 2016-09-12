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
                @"INSERT [integrator].[translation_rules] ([source], [target], [source_property], [target_property], [is_sync_key]) VALUES (@source, @target, @source_property, @target_property, @is_sync_key); SELECT @@ROWCOUNT;";
            private const string UpdateCommandText =
                @"UPDATE [integrator].[translation_rules]" +
                @" SET [source] = @new_source, [target] = @new_target, [source_property] = @new_source_property, [target_property] = @target_property, [is_sync_key] = @is_sync_key " +
                @" WHERE [source] = @old_source AND [target] = @old_target AND [source_property] = @old_source_property; " +
                @"SELECT @@ROWCOUNT;";
            private const string DeleteCommandText =
                @"DELETE [integrator].[translation_rules] WHERE [source] = @old_source AND [target] = @old_target AND [source_property] = @old_source_property; SELECT @@ROWCOUNT;";

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
                    parameter = new SqlParameter("source_property", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.source_property.Identity;
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter("target_property", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.target_property.Identity;
                    command.Parameters.Add(parameter);
                    parameter = new SqlParameter("is_sync_key", SqlDbType.Bit);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.is_sync_key;
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

                    parameter = new SqlParameter("new_source_property", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.source_property.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("old_source_property", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.source_property_old.Identity;
                    command.Parameters.Add(parameter);
                    
                    parameter = new SqlParameter("target_property", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.target_property.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("is_sync_key", SqlDbType.Bit);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.is_sync_key;
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

                    parameter = new SqlParameter("old_source_property", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.source_property_old.Identity;
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
