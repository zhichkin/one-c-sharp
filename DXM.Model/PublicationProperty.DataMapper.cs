using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using Zhichkin.Metadata.Model;
using Zhichkin.ORM;

namespace Zhichkin.DXM.Model
{
    public partial class PublicationProperty
    {
        public sealed class DataMapper : IDataMapper
        {
            # region " SQL "
            private const string SelectCommandText = @"SELECT [publication], [name], [type], [value], [purpose], [version] FROM [dxm].[publication_properties] WHERE [key] = @key";
            private const string InsertCommandText =
                @"DECLARE @result table([version] binary(8)); " +
                @"INSERT [dxm].[publication_properties] ([key], [publication], [name], [type], [value], [purpose]) " +
                @"OUTPUT inserted.[version] INTO @result " +
                @"VALUES (@key, @publication, @name, @type, @value, @purpose); " +
                @"IF @@ROWCOUNT > 0 SELECT [version] FROM @result;";
            private const string UpdateCommandText =
                @"DECLARE @rows_affected int; DECLARE @result table([version] binary(8)); " +
                @"UPDATE [dxm].[publication_properties]" +
                @" SET [publication] = @publication, [name] = @name, [type] = @type, [value] = @value, [purpose] = @purpose " +
                @"OUTPUT inserted.[version] INTO @result" +
                @" WHERE [key] = @key AND [version] = @version; " +
                @"SET @rows_affected = @@ROWCOUNT; " +
                @"IF (@rows_affected = 0) " +
                @"BEGIN " +
                @"  INSERT @result ([version]) SELECT [version] FROM [dxm].[publication_properties] WHERE [key] = @key; " +
                @"END " +
                @"SELECT @rows_affected, [version] FROM @result;";
            private const string DeleteCommandText =
                @"DELETE [dxm].[publication_properties] WHERE [key] = @key " +
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
                PublicationProperty e = (PublicationProperty)entity;
                IPersistentContext metadata = MetadataPersistentContext.Current;
                IBinaryFormatter formatter = new BinaryFormatter();

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
                        e._owner = Factory.New<Publication>(reader.GetGuid(0));
                        e.name = reader.GetString(1);
                        e._type = metadata.Factory.New<Entity>(reader.GetGuid(2));
                        e._value = formatter.Deserialize(new MemoryStream((byte[])reader[3]), e._type);
                        e._purpose = (PublicationPropertyPurpose)reader.GetInt32(4);
                        e.version = (byte[])reader[5];
                        
                        ok = true;
                    }

                    reader.Close(); connection.Close();
                }

                if (!ok) throw new ApplicationException("Error executing select command.");
            }
            void IDataMapper.Insert(IPersistent entity)
            {
                PublicationProperty e = (PublicationProperty)entity;
                IBinaryFormatter formatter = new BinaryFormatter();

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

                    parameter = new SqlParameter("publication", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e._owner == null) ? Guid.Empty : e._owner.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("name", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.name == null) ? string.Empty : e.name;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("type", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e._type == null) ? Guid.Empty : e._type.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("value", SqlDbType.VarBinary);
                    parameter.Direction = ParameterDirection.Input;
                    MemoryStream stream = new MemoryStream();
                    formatter.Serialize(stream, e._value);
                    parameter.Value = stream.ToArray();
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("purpose", SqlDbType.Int);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (int)e._purpose;
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
                PublicationProperty e = (PublicationProperty)entity;
                IBinaryFormatter formatter = new BinaryFormatter();

                bool ok = false; int rows_affected = 0;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command  = connection.CreateCommand();
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

                    parameter = new SqlParameter("publication", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e._owner == null) ? Guid.Empty : e._owner.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("name", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.name == null) ? string.Empty : e.name;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("type", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e._type == null) ? Guid.Empty : e._type.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("value", SqlDbType.VarBinary);
                    parameter.Direction = ParameterDirection.Input;
                    MemoryStream stream = new MemoryStream();
                    formatter.Serialize(stream, e._value);
                    parameter.Value = stream.ToArray();
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("purpose", SqlDbType.Int);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (int)e._purpose;
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
                PublicationProperty e = (PublicationProperty)entity;

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
