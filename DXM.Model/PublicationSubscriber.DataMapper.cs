using System;
using System.Data;
using System.Data.SqlClient;
using Zhichkin.ORM;

namespace Zhichkin.DXM.Model
{
    public partial class PublicationSubscriber
    {
        public sealed class DataMapper : IDataMapper
        {
            # region " CRUD SQL commands "
            private const string InsertCommandText =
                @"INSERT [dxm].[subscribers] ([publication], [subscriber]) VALUES (@publication, @subscriber); SELECT @@ROWCOUNT;";
            private const string UpdateCommandText =
                @"UPDATE [dxm].[subscribers]" +
                @" SET [publication] = @new_publication, [subscriber] = @new_subscriber" +
                @" WHERE [publication] = @old_publication AND [subscriber] = @old_subscriber; " +
                @"SELECT @@ROWCOUNT;";
            private const string DeleteCommandText =
                @"DELETE [dxm].[subscribers] WHERE [publication] = @old_publication AND [subscriber] = @old_subscriber; SELECT @@ROWCOUNT;";
            # endregion

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
                PublicationSubscriber e = (PublicationSubscriber)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = InsertCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("publication", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e._Publication.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("subscriber", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e._Subscriber.Identity;
                    command.Parameters.Add(parameter);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read()) { ok = (int)reader[0] > 0; }

                    reader.Close(); connection.Close();
                }

                if (!ok) throw new ApplicationException("Error executing insert command.");
            }
            void IDataMapper.Update(IPersistent entity)
            {
                PublicationSubscriber e = (PublicationSubscriber)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = UpdateCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("new_publication", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e._Publication.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("new_subscriber", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e._Subscriber.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("old_publication", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e._Old_Publication.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("old_subscriber", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e._Old_Subscriber.Identity;
                    command.Parameters.Add(parameter);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read()) { ok = (int)reader[0] > 0; }

                    reader.Close(); connection.Close();
                }

                if (!ok) throw new ApplicationException("Error executing update command.");
            }
            void IDataMapper.Delete(IPersistent entity)
            {
                PublicationSubscriber e = (PublicationSubscriber)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = DeleteCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("old_publication", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e._Old_Publication.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("old_subscriber", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e._Old_Subscriber.Identity;
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
