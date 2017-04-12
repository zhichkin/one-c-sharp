using System;
using System.Data.SqlClient;
using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public partial class ReferenceProxy
    {
        public sealed class DataMapper : IDataMapper
        {
            private readonly Entity _owner;
            private readonly string _connection_string;
            private readonly string _table_name;
            public DataMapper(Entity owner)
            {
                _owner = owner;
                _connection_string = owner.InfoBase.ConnectionString;
                _table_name = owner.MainTable.Name;
            }

            void IDataMapper.Select(IPersistent entity)
            {
                ReferenceProxy e = (ReferenceProxy)entity;

                string sql = string.Format("SELECT [_Description] FROM {0} WHERE [_IDRRef] = @key", _table_name);

                bool ok = false;
                using (SqlConnection connection = new SqlConnection(_connection_string))
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("key", e.identity);
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                e._presentation = reader.GetString(0);
                                ok = true;
                            }
                        }
                    }
                }
                if (!ok)
                {
                    e._presentation = string.Format("Объект не найден {0}{1}:{2}{3}", "{", e._entity.Code.ToString(), e.identity.ToString(), "}");
                }
            }
            void IDataMapper.Insert(IPersistent entity)
            {
                throw new NotSupportedException();
            }
            void IDataMapper.Update(IPersistent entity)
            {
                throw new NotSupportedException();
            }
            void IDataMapper.Delete(IPersistent entity)
            {
                throw new NotSupportedException();
            }
        }
    }
}