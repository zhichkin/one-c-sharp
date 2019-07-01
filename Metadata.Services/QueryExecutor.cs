using System.Collections.Generic;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Metadata.Services
{
    public sealed class QueryExecutor
    {
        private Dictionary<string, PropertyReferenceTranslator> propertyTranslators = new Dictionary<string, PropertyReferenceTranslator>();

        public void BuildSQLCommand(QueryExpression query)
        {
            foreach (HermesModel expression in query.Expressions)
            {
                VisitExpression(expression);
            }
        }
        private void VisitExpression(HermesModel expression)
        {
            if (expression is SelectStatement)
            {
                VisitSelectStatement((SelectStatement)expression);
            }
        }
        private void VisitSelectStatement(SelectStatement expression)
        {
            foreach (PropertyExpression property in expression.SELECT)
            {
                VisitPropertyExpression(property);
            }
            foreach (TableExpression table in expression.FROM)
            {
                VisitTableExpression(table);
            }
            VisitBooleanFunction(expression.WHERE);
        }
        private void VisitPropertyExpression(PropertyExpression expression)
        {
            PropertyReference property = expression.Expression as PropertyReference;
            if (property == null) return;

            PropertyReferenceTranslator translator = new PropertyReferenceTranslator(property);
            propertyTranslators.Add(expression.Alias, translator);
            string sql = $"{translator.ToSQL()} AS [{expression.Alias}]";
        }
        private void VisitTableExpression(TableExpression table)
        {
            // TODO
        }
        private void VisitBooleanFunction(BooleanFunction expression)
        {
            // TODO
        }
    }
}

//private void PrepareSelectEntityDataCommand(Subscription subscription, AggregateItem item, Guid aggregate, SqlCommand command)
//{
//    command.CommandType = CommandType.Text;
//    command.CommandText = GetSelectEntityDataScript(subscription, item);
//    command.Parameters.Clear();
//    AddParameters(command, item, aggregate);
//}
//private string GetSelectEntityDataScript(Subscription subscription, AggregateItem item)
//{
//    string sql = "SELECT {0} FROM [{1}] WHERE {2};";

//    string table = subscription.Publisher.Entity.MainTable.Name;
//    StringBuilder fields = new StringBuilder();
//    StringBuilder where = new StringBuilder();

//    AddFields(fields, subscription);
//    AddConditions(where, item.Connector);

//    sql = string.Format(sql, fields.ToString(), table, where.ToString());

//    return sql;
//}
//private void AddFields(StringBuilder fields, Subscription subscription)
//{
//    foreach (string name in subscription.Mappings.Keys)
//    {
//        if (fields.Length > 0) fields.Append(", ");
//        fields.AppendFormat("[{0}]", name);
//    }
//}
//private void AddConditions(StringBuilder where, Property connector)
//{
//    foreach (Field field in connector.Fields)
//    {
//        if (field.Purpose == FieldPurpose.Locator)
//        {
//            if (where.Length > 0) where.Append(" AND ");
//            where.AppendFormat("[{0}] = @locator", field.Name);
//        }
//        if (field.Purpose == FieldPurpose.TypeCode)
//        {
//            if (where.Length > 0) where.Append(" AND ");
//            where.AppendFormat("[{0}] = @code", field.Name);
//        }
//        if (field.Purpose == FieldPurpose.Object || field.Purpose == FieldPurpose.Value)
//        {
//            if (where.Length > 0) where.Append(" AND ");
//            where.AppendFormat("[{0}] = @value", field.Name);
//        }
//    }
//}
//private void AddParameters(SqlCommand command, AggregateItem item, Guid aggregate)
//{
//    foreach (Field field in item.Connector.Fields)
//    {
//        if (field.Purpose == FieldPurpose.Locator)
//        {
//            command.Parameters.AddWithValue("locator", new byte[] { 0x08 }); // reference
//        }
//        if (field.Purpose == FieldPurpose.TypeCode)
//        {
//            command.Parameters.AddWithValue("code", Utilities.GetByteArray(item.Aggregate.Code));
//        }
//        if (field.Purpose == FieldPurpose.Object || field.Purpose == FieldPurpose.Value)
//        {
//            command.Parameters.AddWithValue("value", aggregate.ToByteArray());
//        }
//    }
//}
