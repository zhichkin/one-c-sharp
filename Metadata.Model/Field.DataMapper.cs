using System;
using System.Data;
using System.Data.SqlClient;
using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public partial class Field
    {
        public sealed class DataMapper : IDataMapper
        {
            private const string SelectCommandText =
                @"SELECT [version], [name], [table], [property], [purpose], [type_name], [length], [precision], [scale], [is_nullable], [is_primary_key], [key_ordinal] FROM [metadata].[fields] WHERE [key] = @key";
            private const string InsertCommandText =
                @"DECLARE @result table([version] binary(8)); " +
                @"INSERT [metadata].[fields] ([key], [name], [table], [property], [purpose], [type_name], [length], [precision], [scale], [is_nullable], [is_primary_key], [key_ordinal]) " +
                @"OUTPUT inserted.[version] INTO @result " +
                @"VALUES (@key, @name, @table, @property, @purpose, @type_name, @length, @precision, @scale, @is_nullable, @is_primary_key, @key_ordinal); " +
                @"IF @@ROWCOUNT > 0 SELECT [version] FROM @result;";
            private const string UpdateCommandText =
                @"DECLARE @rows_affected int; DECLARE @result table([version] binary(8)); " +
                @"UPDATE [metadata].[fields] SET [name] = @name, [table] = @table, [property] = @property, [purpose] = @purpose, [type_name] = @type_name, [length] = @length, [precision] = @precision, [scale] = @scale, [is_nullable] = @is_nullable, [is_primary_key] = @is_primary_key, [key_ordinal] = @key_ordinal " +
                @"OUTPUT inserted.[version] INTO @result" +
                @" WHERE [key] = @key AND [version] = @version; " +
                @"SET @rows_affected = @@ROWCOUNT; " +
                @"IF (@rows_affected = 0) " +
                @"BEGIN " +
                @"  INSERT @result ([version]) SELECT [version] FROM [metadata].[fields] WHERE [key] = @key; " +
                @"END " +
                @"SELECT @rows_affected, [version] FROM @result;";
            private const string DeleteCommandText =
                @"DELETE [metadata].[fields] WHERE [key] = @key " +
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
                Field e = (Field)entity;

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
                        e.version        = (byte[])reader[0];
                        e.name           = (string)reader[1];
                        e.table          = Factory.New<Table>((Guid)reader[2]);
                        e.property       = Factory.New<Property>((Guid)reader[3]);
                        e.purpose        = (FieldPurpose)reader[4];
                        e.type_name      = (string)reader[5];
                        e.length         = (int)reader[6];
                        e.precision      = (int)reader[7];
                        e.scale          = (int)reader[8];
                        e.is_nullable    = (bool)reader[9];
                        e.is_primary_key = (bool)reader[10];
                        e.key_ordinal    = (byte)reader[11];

                        ok = true;
                    }

                    reader.Close(); connection.Close();
                }

                if (!ok) throw new ApplicationException("Error executing select command.");
            }

            void IDataMapper.Insert(IPersistent entity)
            {
                Field e = (Field)entity;

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

                    parameter = new SqlParameter("name", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.name == null) ? string.Empty : e.name;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("table", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.table == null) ? Guid.Empty : e.table.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("property", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.property == null) ? Guid.Empty : e.property.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("purpose", SqlDbType.Int);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (int)e.purpose;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("type_name", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.type_name == null) ? string.Empty : e.type_name;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("length", SqlDbType.Int);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.length;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("precision", SqlDbType.Int);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.precision;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("scale", SqlDbType.Int);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.scale;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("is_nullable", SqlDbType.Bit);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.is_nullable;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("is_primary_key", SqlDbType.Bit);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.is_primary_key;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("key_ordinal", SqlDbType.TinyInt);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.key_ordinal;
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
                Field e = (Field)entity;

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

                    parameter = new SqlParameter("name", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.name == null) ? string.Empty : e.name;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("table", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.table == null) ? Guid.Empty : e.table.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("property", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.property == null) ? Guid.Empty : e.property.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("purpose", SqlDbType.Int);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (int)e.purpose;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("type_name", SqlDbType.NVarChar);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e.type_name == null) ? string.Empty : e.type_name;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("length", SqlDbType.Int);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.length;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("precision", SqlDbType.Int);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.precision;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("scale", SqlDbType.Int);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.scale;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("is_nullable", SqlDbType.Bit);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.is_nullable;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("is_primary_key", SqlDbType.Bit);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.is_primary_key;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("key_ordinal", SqlDbType.TinyInt);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e.key_ordinal;
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
                Field e = (Field)entity;

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