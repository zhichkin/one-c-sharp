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
        int Count();
        List<dynamic> Select();
        List<dynamic> Select(int pageNumber, int pageSize);
    }

    public sealed class SqlCommandBuilder : ISqlCommandBuilder
    {
        private bool _isReadyForUse = false;
        private readonly Entity _metadata;
        private readonly List<Field> _primaryKey = new List<Field>();
        private readonly Dictionary<Property, IList<Field>> _info = new Dictionary<Property, IList<Field>>();
        private string _select = string.Empty;
        private string _paging = string.Empty;
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
        public int Count()
        {
            if (!_isReadyForUse) throw new InvalidOperationException("Call \"Build\" method first!");
            int result = 0;
            string sql = string.Format("SELECT COUNT(*) FROM [{0}];", _metadata.MainTable.Name);
            using (SqlConnection connection = new SqlConnection(_metadata.InfoBase.ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                result = (int)command.ExecuteScalar();
            }
            return result;
        }
        public List<dynamic> Select(int pageNumber, int pageSize)
        {
            if (!_isReadyForUse) throw new InvalidOperationException("Call \"Build\" method first!");

            DataTranslator translator = new DataTranslator(_metadata);

            List<dynamic> result = new List<dynamic>();
            using (SqlConnection connection = new SqlConnection(_metadata.InfoBase.ConnectionString))
            using (SqlCommand command = new SqlCommand(_paging, connection))
            {
                connection.Open();
                command.Parameters.AddWithValue("start", (pageNumber - 1) * pageSize + 1);
                command.Parameters.AddWithValue("end", pageNumber * pageSize);
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
            SetupLookups();
            BuildSelectStatement();
            BuildPagingSelect();
            _isReadyForUse = true;
            return this;
        }
        private void SetupLookups()
        {
            foreach (Property property in _metadata.Properties)
            {
                IList<Field> fields = property.Fields;
                _info.Add(property, fields);
                for (int i = 0; i < fields.Count; i++)
                {
                    Field field = fields[i];
                    if (field.IsPrimaryKey) _primaryKey.Add(field);
                }
            }
        }
        private void BuildSelectStatement()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append(GetSelect());
            sb.Append(" FROM [" + _metadata.MainTable.Name + "];");
            _select = sb.ToString();
        }
        private void BuildPagingSelect()
        {
            string template = "SELECT {0} FROM (SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS RowNumber, {0} FROM [{1}]) AS T WHERE T.RowNumber BETWEEN @start AND @end;";
            _paging = string.Format(template,
                GetSelect(),
                _metadata.MainTable.Name);
        }
        private string GetSelect()
        {
            StringBuilder sb = new StringBuilder();
            bool isFirst = true;
            foreach (Property property in _info.Keys)
            {
                IList<Field> fields = _info[property];
                for (int i = 0; i < fields.Count; i++)
                {
                    Field field = fields[i];
                    if (!isFirst) sb.Append(", ");
                    if (isFirst) { isFirst = !isFirst; }
                    sb.Append("[" + field.Name + "]");
                }
            }
            return sb.ToString();
        }
    }
}
