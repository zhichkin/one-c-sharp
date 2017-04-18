using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Text;

namespace Zhichkin.Metadata.Model
{
    public interface ISqlCommandBuilder
    {
        Entity Metadata { get; }
        ISqlCommandBuilder Build();
        List<dynamic> Select();
    }

    public sealed class SqlCommandBuilder : ISqlCommandBuilder
    {
        private bool _isReadyForUse = false;
        private readonly Entity _metadata;
        private readonly List<Field> _primaryKey = new List<Field>();
        private readonly Dictionary<Property, IList<Field>> _info = new Dictionary<Property, IList<Field>>();
        private string _select = string.Empty;
        public SqlCommandBuilder(Entity metadata)
        {
            _metadata = metadata;
        }
        public Entity Metadata { get { return _metadata; } }
        public List<dynamic> Select()
        {
            if (!_isReadyForUse) throw new InvalidOperationException("Call \"Build\" method first!");

            DataTranslator translator = new DataTranslator(_metadata);

            List<dynamic> result = new List<dynamic>();
            using (SqlConnection connection = new SqlConnection(_metadata.InfoBase.ConnectionString))
            using (SqlCommand command = new SqlCommand(_select, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dynamic item = new ExpandoObject();
                        translator.Translate(reader, (IDictionary<string, object>)item);
                        result.Add(item);
                    }
                }
            }
            return result;
        }
        public ISqlCommandBuilder Build()
        {
            if (_isReadyForUse) return this;
            BuildSelectStatement();
            _isReadyForUse = true;
            return this;
        }
        private void BuildSelectStatement()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            bool isFirst = true;
            foreach (Property property in _metadata.Properties)
            {
                IList<Field> fields = property.Fields;
                _info.Add(property, fields);
                for (int i = 0; i < fields.Count; i++)
                {
                    Field field = fields[i];
                    if (field.IsPrimaryKey) _primaryKey.Add(field);
                    if (!isFirst) sb.Append(", ");
                    if (isFirst) { isFirst = !isFirst; }
                    sb.Append("[" + field.Name + "]");
                }
            }
            sb.Append(" FROM [" + _metadata.MainTable.Name + "];");
            //for (int i = 0; i < _primaryKey.Count; i++)
            //{
            //    Field field = _primaryKey[i];
            //    if (counter > 0) sb.AppendFormat("[{0}] =  AND ");
            //    sb.Append("[" + field.Name + "]");
            //}
            _select = sb.ToString();
        }
    }
}
