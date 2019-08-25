using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public interface ISqlCommandBuilder
    {
        Entity Metadata { get; }
        ISqlCommandBuilder Build();
        int Count();
        List<dynamic> Select();
        List<dynamic> Select(int pageNumber, int pageSize, List<FilterParameter> filter);
    }

    public sealed class SqlCommandBuilder : ISqlCommandBuilder
    {
        private bool _isReadyForUse = false;
        private readonly Entity _metadata;
        private readonly List<Field> _primaryKey = new List<Field>();
        private readonly Dictionary<Property, IList<Field>> _info = new Dictionary<Property, IList<Field>>();
        private readonly Dictionary<FilterOperator, string> _FilterOperatorsStringValues = new Dictionary<FilterOperator, string>()
        {
            { FilterOperator.Equal,         "=" },
            {FilterOperator.NotEqual,       "<>" },
            {FilterOperator.Greater,        ">" },
            {FilterOperator.GreaterOrEqual, ">=" },
            {FilterOperator.Less,           "<" },
            {FilterOperator.LessOrEqual,    "<=" },
            {FilterOperator.Contains,       "LIKE" }, // IN({0})
            {FilterOperator.Between,        "BETWEEN {0} AND {1}" }
        };
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
        public List<dynamic> Select(int pageNumber, int pageSize, List<FilterParameter> filter)
        {
            if (!_isReadyForUse) throw new InvalidOperationException("Call \"Build\" method first!");

            DataTranslator translator = new DataTranslator(_metadata);

            string sql = BuildPagingSelect(filter);

            List<dynamic> result = new List<dynamic>();
            using (SqlConnection connection = new SqlConnection(_metadata.InfoBase.ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.Parameters.AddWithValue("start", (pageNumber - 1) * pageSize + 1);
                command.Parameters.AddWithValue("end", pageNumber * pageSize);
                AddFilterParameters(filter, command.Parameters);
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
        private string BuildPagingSelect(List<FilterParameter> filter)
        {
            string template = "SELECT {0} FROM (SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS RowNumber, {0} FROM [{1}] {2}) AS T WHERE T.RowNumber BETWEEN @start AND @end;";
            string WHERE = BuildWhere(filter);
            return string.Format(template,
                GetSelect(),
                _metadata.MainTable.Name,
                WHERE);
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
        private Property GetPropertyByName(string name)
        {
            return _metadata.Properties.Where(p => p.Name == name).FirstOrDefault();
        }
        private string BuildWhere(List<FilterParameter> filters)
        {
            if (filters == null || filters.Count == 0) return string.Empty;

            StringBuilder sb = new StringBuilder("WHERE");
            
            foreach (FilterParameter filter in filters)
            {
                Property property = GetPropertyByName(filter.Name);
                if (property == null) continue;

                IList<Field> fields = property.Fields;
                if (fields.Count == 0) continue;

                sb.Append(" ").Append(GetWhere(fields, filter));
            }
            return sb.ToString();
        }
        private string GetWhere(IList<Field> fields, FilterParameter filter)
        {
            StringBuilder sb = new StringBuilder();
            string template = "[{0}] {1} @{2}";
            foreach (Field field in fields)
            {
                sb.AppendFormat(template,
                    field.Name,
                    FilterOperatorToString(filter.Operator),
                    field.Name);
            }
            return sb.ToString();
        }
        private string FilterOperatorToString(FilterOperator _operator)
        {
            return _FilterOperatorsStringValues[_operator];
        }
        private void AddFilterParameters(List<FilterParameter> filters, SqlParameterCollection parameters)
        {
            if (filters == null) return;

            foreach (FilterParameter filter in filters)
            {
                Field field = GetValueField(filter);

                if (filter.Value == null)
                {
                    parameters.AddWithValue(field.Name, DBNull.Value);
                    return;
                }

                Type type = filter.Value.GetType();
                if (type == typeof(string))
                {
                    if (filter.Operator == FilterOperator.Contains)
                    {
                        parameters.AddWithValue(field.Name, (string)filter.Value + "%");
                    }
                    else
                    {
                        parameters.AddWithValue(field.Name, (string)filter.Value);
                    }
                }
                else if (filter.Value is ReferenceObject)
                {
                    parameters.AddWithValue(field.Name, ((ReferenceObject)filter.Value).Identity);
                }
                else
                {
                    parameters.AddWithValue(field.Name, filter.Value);
                }
            }
        }
        private Field GetValueField(FilterParameter filter)
        {
            Property property = GetPropertyByName(filter.Name);
            IList<Field> fields = property.Fields;
            FieldPurpose purpose = FieldPurpose.Value;
            if (filter.Value == null || filter.Value is ReferenceObject) purpose = FieldPurpose.Object;
            foreach (Field field in fields)
            {
                if (field.Purpose == purpose)
                {
                    return field;
                }
            }
            return fields[0];
        }
    }
}
