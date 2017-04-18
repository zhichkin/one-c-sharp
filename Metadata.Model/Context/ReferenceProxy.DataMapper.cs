using System;
using System.Linq;
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

                string sql = string.Empty;
                bool hasNumber = false;
                if (_owner.Namespace.Name == "Справочник")
                {
                    sql = string.Format("SELECT [_Description] FROM {0} WHERE [_IDRRef] = @key", _table_name);
                }
                else if (_owner.Namespace.Name == "Документ")
                {
                    Field field = _owner.MainTable.Fields.Where(f => f.Name == "_Number").FirstOrDefault();
                    hasNumber = (field != null);
                    if (hasNumber)
                    {
                        sql = string.Format("SELECT [_Date_Time], [_Number] FROM {0} WHERE [_IDRRef] = @key", _table_name);
                    }
                    else
                    {
                        sql = string.Format("SELECT [_Date_Time] FROM {0} WHERE [_IDRRef] = @key", _table_name);
                    }
                }
                else
                {
                    e._presentation = e.identity.ToString();
                    return;
                }

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
                                if (_owner.Namespace.Name == "Справочник")
                                {
                                    e._presentation = reader.GetString(0);
                                }
                                else if (_owner.Namespace.Name == "Документ")
                                {
                                    if (hasNumber)
                                    {
                                        e._presentation = string.Format("{0} {1} от {2}",
                                            _owner.Alias,
                                            reader.GetString(1),
                                            reader.GetDateTime(0).ToString("dd.MM.yyyy hh:mm:ss"));
                                    }
                                    else
                                    {
                                        e._presentation = string.Format("{0} от {1}",
                                            _owner.Alias,
                                            reader.GetDateTime(0).ToString("dd.MM.yyyy hh:mm:ss"));
                                    }
                                }
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