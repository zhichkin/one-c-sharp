using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Metadata.Services
{
    public sealed class QueryExecutor
    {
        QueryExpression query;
        StringBuilder sql = new StringBuilder();
        private Dictionary<string, PropertyReferenceManager> propertyManagers = new Dictionary<string, PropertyReferenceManager>();

        public QueryExecutor(QueryExpression query)
        {
            this.query = query;
        }
        public string ToSQL() { return sql.ToString(); }
        public IEnumerable Execute()
        {
            string ConnectionString = MetadataPersistentContext.Current.ConnectionString;
            PropertyReferenceManager manager;

            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql.ToString(), connection))
            {
                connection.Open();

                if (query.Parameters != null)
                {
                    foreach (ParameterExpression parameter in query.Parameters)
                    {
                        if (parameter.Value is ReferenceProxy)
                        {
                            ReferenceProxy parameterValue = (ReferenceProxy)parameter.Value;
                            if (parameterValue.Type.Namespace.Name == "MetaModel")
                            {
                                command.Parameters.AddWithValue(parameter.Name, parameterValue.Identity);
                            }
                            else
                            {
                                command.Parameters.AddWithValue(parameter.Name, parameterValue.Identity.ToByteArray());
                            }
                        }
                        else
                        {
                            command.Parameters.AddWithValue(parameter.Name, parameter.Value);
                        }
                    }
                }

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Dictionary<string, object> item = new Dictionary<string, object>();
                        foreach (KeyValuePair<string, PropertyReferenceManager> pm in propertyManagers)
                        {
                            manager = pm.Value;
                            string name = pm.Key;
                            object value = manager.GetValue(reader);
                            item.Add(pm.Key, value);
                        }
                        result.Add(item);
                    }
                }
            }
            return result.ToDataSource();
        }
        public List<Dictionary<string, object>> ExecuteAsRowData()
        {
            string ConnectionString = MetadataPersistentContext.Current.ConnectionString;
            PropertyReferenceManager manager;

            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql.ToString(), connection))
            {
                connection.Open();

                if (query.Parameters != null)
                {
                    foreach (ParameterExpression parameter in query.Parameters)
                    {
                        if (parameter.Value is ReferenceProxy)
                        {
                            ReferenceProxy parameterValue = (ReferenceProxy)parameter.Value;
                            if (parameterValue.Type.Namespace.Name == "MetaModel")
                            {
                                command.Parameters.AddWithValue(parameter.Name, parameterValue.Identity);
                            }
                            else
                            {
                                command.Parameters.AddWithValue(parameter.Name, parameterValue.Identity.ToByteArray());
                            }
                        }
                        else
                        {
                            command.Parameters.AddWithValue(parameter.Name, parameter.Value);
                        }
                    }
                }

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Dictionary<string, object> item = new Dictionary<string, object>();
                        foreach (KeyValuePair<string, PropertyReferenceManager> pm in propertyManagers)
                        {
                            manager = pm.Value;
                            string name = pm.Key;
                            object value = manager.GetValue(reader);
                            item.Add(pm.Key, value);
                        }
                        result.Add(item);
                    }
                }
            }
            return result;
        }

        public QueryExecutor Build()
        {
            foreach (HermesModel expression in query.Expressions)
            {
                VisitExpression(expression);
            }
            return this;
        }
        private void VisitExpression(HermesModel expression)
        {
            if (expression is SelectStatement)
            {
                VisitSelectStatement((SelectStatement)expression);
            }
            else if (expression is PropertyReference)
            {
                VisitPropertyReference((PropertyReference)expression);
            }
            else if (expression is ParameterExpression)
            {
                VisitParameterExpression((ParameterExpression)expression);
            }
        }
        private void VisitSelectStatement(SelectStatement expression)
        {
            sql.Append("SELECT");
            int currentOrdinal = 0;
            foreach (PropertyExpression property in expression.SELECT)
            {
                VisitPropertyExpression(property, ref currentOrdinal);
            }

            sql.Append("\nFROM");
            foreach (TableExpression table in expression.FROM)
            {
                VisitTableExpression(table);
            }

            if (expression.WHERE != null)
            {
                sql.Append("\nWHERE");
                VisitBooleanFunction(expression.WHERE);
            }
        }
        private void VisitPropertyExpression(PropertyExpression expression, ref int currentOrdinal)
        {
            PropertyReference property = expression.Expression as PropertyReference;
            if (property == null) return;

            if (currentOrdinal == 0) { sql.Append("\n"); }

            PropertyReferenceManager manager = new PropertyReferenceManager(property);
            manager.Prepare(ref currentOrdinal);
            propertyManagers.Add(expression.Alias, manager);

            sql.Append($"\t{manager.ToSQL()}");
        }
        private void VisitTableExpression(TableExpression table)
        {
            if (table is JoinExpression)
            {
                JoinExpression jt = (JoinExpression)table;
                sql.Append($"{Environment.NewLine}\t{jt.JoinType} {jt.Entity.MainTable.FullName} AS [{jt.Alias}]");
                if (jt.Hint != HintTypes.NoneHint)
                {
                    sql.Append($" WITH({jt.Hint})");
                }

                sql.Append("\n\tON");
                VisitBooleanFunction(jt.ON);
            }
            else if (table is TableExpression)
            {
                sql.Append($"{Environment.NewLine}\t{table.Entity.MainTable.FullName} AS [{table.Alias}]");
                if (table.Hint != HintTypes.NoneHint)
                {
                    sql.Append($" WITH({table.Hint})");
                }
            }
        }
        private void VisitBooleanFunction(BooleanFunction expression)
        {
            if (expression is BooleanOperator)
            {
                VisitBooleanOperator((BooleanOperator)expression);
            }
            else if (expression is ComparisonOperator)
            {
                VisitComparisonOperator((ComparisonOperator)expression);
            }
        }
        private void VisitBooleanOperator(BooleanOperator expression)
        {
            int counter = 0;

            sql.Append("\n\t(");
            foreach (BooleanFunction function in expression.Operands)
            {
                sql.Append("\n\t");
                if (counter > 0)
                {
                    sql.Append($"{expression.Name} ");
                }
                VisitBooleanFunction(function);
                counter++;
            }
            sql.Append("\n\t)");
        }
        private void VisitComparisonOperator(ComparisonOperator expression)
        {
            if (expression.IsRoot)
            {
                sql.Append("\n\t(");
            }
            VisitExpression(expression.LeftExpression);
            sql.Append($" {expression.Name} ");
            VisitExpression(expression.RightExpression);
            if (expression.IsRoot)
            {
                sql.Append(")");
            }
        }
        private void VisitPropertyReference(PropertyReference expression)
        {
            Field field = expression.Property.Fields.Where(f => f.Purpose == FieldPurpose.Value).FirstOrDefault();
            if (field == null)
            {
                field = expression.Property.Fields.Where(f => f.Purpose == FieldPurpose.Object).FirstOrDefault();
            }
            if (field == null)
            {
                sql.Append($"[{expression.Table.Alias}].[{expression.Name}]");
            }
            else
            {
                sql.Append($"[{expression.Table.Alias}].[{field.Name}]");
            }
        }
        private void VisitParameterExpression(ParameterExpression expression)
        {
            sql.Append($"@{expression.Name}");
        }
    }
}