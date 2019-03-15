using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Zhichkin.Hermes.Infrastructure;
using Zhichkin.Metadata.Model;
using Zhichkin.ORM;

namespace Zhichkin.Hermes.Services
{
    public sealed class DocumentsTreeService
    {
        private const string CONST_ConnectionStringName = "TEST";
        private const string CONST_MetadataCatalogSettingName = "MetadataCatalog";
        private string connection_string;
        private string temporary_catalog;

        public DocumentsTreeService()
        {
            connection_string = ConfigurationManager.ConnectionStrings[CONST_ConnectionStringName].ConnectionString;
            temporary_catalog = ConfigurationManager.AppSettings[CONST_MetadataCatalogSettingName];
        }

        public async Task BuildDocumentsTree(IEntityInfo document, DateTime period, Action<string> notifyStateCallback, Action<MetadataTreeNode> workIsDoneCallback)
        {
            if (document == null) throw new ArgumentNullException("document");

            MetadataTreeNode root = new MetadataTreeNode()
            {
                Name = document.Name,
                MetadataInfo = document
            };

            string message = "Start: " + DateTime.Now.ToUniversalTime();
            WriteToLog(message);
            notifyStateCallback(message);
            await FillChildren(root, period, notifyStateCallback);
            message = "End: " + DateTime.Now.ToUniversalTime();
            WriteToLog(message);
            notifyStateCallback(message);
            notifyStateCallback("");
            workIsDoneCallback(root);
        }
        private async Task FillChildren(MetadataTreeNode parent, DateTime period, Action<string> notifyStateCallback)
        {
            if (!(parent.MetadataInfo is IEntityInfo)) return;

            IPersistentContext context = MetadataPersistentContext.Current;
            using (SqlConnection connection = new SqlConnection(context.ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[get_metadata_tree_node_children]";
                command.Parameters.AddWithValue("entity", parent.MetadataInfo.Identity);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Guid current_ns_key = Guid.Empty;
                    Namespace current_ns = null;
                    MetadataTreeNode current_ns_node = null;

                    Guid current_entity_key = Guid.Empty;
                    Entity current_entity = null;
                    MetadataTreeNode current_entity_node = null;

                    Guid current_nested_key = Guid.Empty;
                    Entity current_nested = null;
                    MetadataTreeNode current_nested_node = null;

                    Property current_property = null;

                    while (reader.Read())
                    {
                        Guid key = (Guid)reader["NamespaceKey"];
                        if (current_ns_key != key)
                        {
                            current_ns_key = key;
                            current_ns = context.Factory.New<Namespace>(key);
                            current_ns_node = new MetadataTreeNode()
                            {
                                Name = current_ns.Name,
                                Parent = parent,
                                MetadataInfo = current_ns
                            };
                            parent.Children.Add(current_ns_node);
                        }

                        key = (Guid)reader["EntityKey"];
                        if (current_entity_key != key)
                        {
                            current_entity_key = key;
                            current_entity = context.Factory.New<Entity>(key);
                            current_entity_node = new MetadataTreeNode()
                            {
                                Name = current_entity.Name,
                                Parent = current_ns_node,
                                MetadataInfo = current_entity,
                                Filter = new BooleanExpression()
                                {
                                    ExpressionType = BooleanExpressionType.OR
                                }
                            };
                            current_ns_node.Children.Add(current_entity_node);
                        }

                        key = (Guid)reader["NestedEntityKey"];
                        if (current_nested_key != key)
                        {
                            if (key == Guid.Empty)
                            {
                                current_nested_key = Guid.Empty;
                                current_nested = null;
                                current_nested_node = null;
                            }
                            else
                            {
                                current_nested_key = key;
                                current_nested = context.Factory.New<Entity>(key);
                                current_nested_node = new MetadataTreeNode()
                                {
                                    Name = current_nested.Name,
                                    Parent = current_entity_node,
                                    MetadataInfo = current_nested,
                                    Filter = new BooleanExpression()
                                    {
                                        ExpressionType = BooleanExpressionType.OR
                                    }
                                };
                                current_entity_node.Children.Add(current_nested_node);
                            }
                        }

                        key = (Guid)reader["PropertyKey"];
                        current_property = context.Factory.New<Property>(key);
                        PropertyExpression px = new PropertyExpression()
                        {
                            Name = current_property.Name,
                            PropertyInfo = current_property
                        };
                        ComparisonExpression ce = new ComparisonExpression()
                        {
                            Name = BooleanExpressionType.Equal.ToString(),
                            ExpressionType = BooleanExpressionType.Equal,
                            LeftExpression = px,
                            RightExpression = null
                        };

                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        if (current_nested_node == null)
                        {
                            ((BooleanExpression)current_entity_node.Filter).Conditions.Add(ce);
                            CountDocuments(current_entity_node, current_property, period);
                        }
                        else
                        {
                            ((BooleanExpression)current_nested_node.Filter).Conditions.Add(ce);
                            CountDocuments(current_nested_node, current_property, period);
                        }
                        sw.Stop();
                        
                        string message = string.Format("{0}.{1}.{2} = {3} seconds",
                            current_ns.Name, current_property.Entity.Name, current_property.Name,
                            sw.Elapsed.TotalSeconds.ToString());
                        notifyStateCallback(message);
                        WriteToLog(message);
                        await Task.Delay(100); // 1/10 секунды
                    }
                }
            }
            await Task.Delay(1);
        }
        private void CountDocuments(MetadataTreeNode node, IPropertyInfo property, DateTime period)
        {
            if (property.Fields == null || property.Fields.Count == 0)
            {
                return;
            }
            foreach (IFieldInfo field in property.Fields)
            {
                if (string.IsNullOrWhiteSpace(field.Name)) return;
            }

            MetadataTreeNode root = node;
            while (root.Parent != null) { root = (MetadataTreeNode)root.Parent; }
            if (root.Keys == null) { root.Keys = GetRootKeys(root, period); }
            if (root.Keys.Count == 0) return;
            
            StringBuilder queryText = new StringBuilder();
            queryText.Append("DECLARE @OrdersXML nvarchar(max) = '");
            queryText.Append("<list>");
            foreach (Guid key in root.Keys)
            {
                queryText.AppendFormat("<item key=\"{0}\"/>", key.ToString());
            }
            queryText.AppendLine("</list>';");
            queryText.AppendLine("DECLARE @xml xml = CAST(@OrdersXML AS xml);");
            queryText.AppendLine("DECLARE @Orders TABLE([key] binary(16) NOT NULL);");
            queryText.AppendLine("INSERT @Orders([key])");
            queryText.AppendLine("SELECT");
            queryText.AppendLine("list.[item].value('@key', 'uniqueidentifier')");
            queryText.AppendLine("FROM @xml.nodes('list/item') AS list([item]);");
            
            string table_name = ((Entity)property.Entity).MainTable.Name;

            string where_filter = "";
            if (property.Fields.Count == 1)
            {
                where_filter = string.Format("S.[{0}] = T.[key]", property.Fields[0].Name);
            }
            else
            {
                string value_name = "";
                string locator_name = "";
                string type_code_name = "";
                int type_code = ((Entity)root.MetadataInfo).Code;
                foreach (IFieldInfo field in property.Fields)
                {
                    switch (field.Purpose)
                    {
                        case FieldPurpose.Object: { value_name = field.Name; break; }
                        case FieldPurpose.Locator: { locator_name = field.Name; break; }
                        case FieldPurpose.TypeCode: { type_code_name = field.Name; break; }
                    }
                }
                if (locator_name == string.Empty)
                {
                    where_filter = string.Format("S.[{0}] = CAST({1} AS binary(4)) AND S.[{2}] = T.[key]",
                        type_code_name, type_code, value_name);
                }
                else
                {
                    where_filter = string.Format("S.[{0}] = 0x08 AND S.[{1}] = CAST({2} AS binary(4)) AND S.[{3}] = T.[key]",
                        locator_name, type_code_name, type_code, value_name);
                }
            }
            string select_statement = string.Format("SELECT COUNT(*) FROM [{0}] AS S INNER JOIN @Orders AS T ON {1};", table_name, where_filter);
            queryText.Append(select_statement);
            WriteToLog(select_statement + Environment.NewLine);

            using (SqlConnection connection = new SqlConnection(connection_string))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = queryText.ToString();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int count = (int)reader[0];
                        node.Count += count;
                        MetadataTreeNode next_node_up = (MetadataTreeNode)node.Parent;
                        while (next_node_up != null && next_node_up != root)
                        {
                            next_node_up.Count += count;
                            next_node_up = (MetadataTreeNode)next_node_up.Parent;
                        }
                    }
                }
            }
        }
        private List<Guid> GetRootKeys(MetadataTreeNode root, DateTime period)
        {
            List<Guid> keys = new List<Guid>();

            string table_name = ((Entity)root.MetadataInfo).MainTable.Name;

            DateTime start_of_period = new DateTime(period.Year, period.Month, period.Day, 0, 0, 0, 0);
            start_of_period = start_of_period.AddYears(2000); // fuck 1C !!!

            StringBuilder query = new StringBuilder();
            query.Append("SELECT [_IDRRef] FROM [" + table_name + "] WHERE [_Date_Time] >= @_Date_Time;");

            using (SqlConnection connection = new SqlConnection(connection_string))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = query.ToString();
                command.Parameters.AddWithValue("_Date_Time", start_of_period); 
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Guid key = new Guid((byte[])reader[0]);
                        keys.Add(key);
                    }
                }
            }
            root.Count = keys.Count;

            WriteToLog("Priod = " + start_of_period.ToString("dd.MM.yyyy HH:mm:ss.ffff", CultureInfo.InvariantCulture));
            WriteToLog(query.ToString());
            WriteToLog("Count = " + root.Count.ToString() + Environment.NewLine);

            return keys;
        }
        private void WriteToLog(string entry)
        {
            string path = Path.Combine(temporary_catalog, "log.txt");
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(entry);
                writer.Close();
            }
        }

        public async Task FindForeignReferences(MetadataTreeNode node)
        {
            IEntityInfo info = node.MetadataInfo as IEntityInfo;
            if (info == null) return;

            Entity entity = (Entity)info;
            List<Property> fkeys = GetForeignKeyProperties(entity);
            string query = BuildForeignKeysQueryText(node, entity, fkeys);

            await Task.Delay(1);
        }
        private List<Property> GetForeignKeyProperties(Entity entity)
        {
            List<Property> list = new List<Property>();

            foreach (Property property in entity.Properties)
            {
                foreach (Field field in property.Fields)
                {
                    if (field.Purpose == FieldPurpose.Object)
                    {
                        list.Add(property);
                        break;
                    }
                }
            }

            return list;
        }
        private string BuildForeignKeysQueryText(MetadataTreeNode node, Entity entity, List<Property> properties)
        {
            string table_name = entity.MainTable.Name;
            string where_clause = BuildWhereClause(node);
            string field_names = BuildFieldNamesText(properties);
            StringBuilder query = new StringBuilder();
            query.AppendLine(string.Format("WITH CTE{0} ({1}) AS", table_name, field_names));
            query.AppendLine("(");
            query.AppendLine(string.Format("SELECT {0} FROM [{1}]", field_names, table_name));
            query.AppendLine(where_clause);
            query.AppendLine(")");

            for (int i = 0; i < properties.Count; i++)
            {
                query.AppendLine(BuildForeignKeyQueryText(entity, properties[i]));
                if (properties.Count > 1 && i < properties.Count - 1)
                {
                    query.AppendLine("UNION");
                }
            }
            query.Append(";");

            return query.ToString();
        }
        private string BuildFieldNamesText(List<Property> properties)
        {
            StringBuilder field_names = new StringBuilder();
            foreach (Property property in properties)
            {
                if (property.Fields.Count == 1)
                {
                    if (field_names.Length > 0)
                    {
                        field_names.Append(", ");
                    }
                    field_names.Append(string.Format("[{0}]", property.Fields[0].Name));
                }
                else
                {
                    foreach (Field field in property.Fields)
                    {
                        if (field.Purpose == FieldPurpose.Object || field.Purpose == FieldPurpose.Locator || field.Purpose == FieldPurpose.TypeCode)
                        {
                            if (field_names.Length > 0)
                            {
                                field_names.Append(", ");
                            }
                            field_names.Append(string.Format("[{0}]", field.Name));
                        }
                    }
                }
            }
            return field_names.ToString();
        }
        private string BuildWhereClause(MetadataTreeNode node)
        {
            if (!(node.Filter.ExpressionType == BooleanExpressionType.OR || node.Filter.ExpressionType == BooleanExpressionType.AND))
            {
                return string.Empty;
            }
            StringBuilder where = new StringBuilder("WHERE ");

            MetadataTreeNode parent = GetParent(node);
            string keys_list_table = BuildParentEntityKeysTableQueryText(parent);

            BooleanExpression filter = (BooleanExpression)node.Filter;
            foreach (IComparisonExpression condition in filter.Conditions)
            {
                Property property = (Property)((PropertyExpression)condition.LeftExpression).PropertyInfo;
            }


            return where.ToString();
        }
        private string BuildForeignKeyQueryText(Entity entity, Property property)
        {
            StringBuilder query = new StringBuilder();

            string type_code = entity.Code.ToString();
            string table_name = entity.MainTable.Name;

            if (property.Fields.Count == 1)
            {
                query.Append(string.Format(
                    "SELECT {0} AS [TYPE_CODE], [{1}] AS [FK_VALUE] FROM CTE{2} WHERE [{1}] > 0x00000000000000000000000000000000",
                    type_code, property.Fields[0].Name, table_name));
            }
            else
            {
                string value_name = string.Empty;
                string locator_name = string.Empty;
                string type_code_name = string.Empty;
                foreach (Field field in property.Fields)
                {
                    switch (field.Purpose)
                    {
                        case FieldPurpose.Object: { value_name = field.Name; break; }
                        case FieldPurpose.Locator: { locator_name = field.Name; break; }
                        case FieldPurpose.TypeCode: { type_code_name = field.Name; break; }
                    }
                }
                if (locator_name == string.Empty)
                {
                    query.Append(string.Format(
                        "SELECT CAST([{0}] AS int) AS [TYPE_CODE], [{1}] AS [FK_VALUE] FROM CTE{2} WHERE [{0}] > 0x00000000 AND [{1}] > 0x00000000000000000000000000000000",
                        type_code_name, value_name, table_name));
                }
                else
                {
                    query.Append(string.Format(
                        "SELECT CAST([{0}] AS int) AS [TYPE_CODE], [{1}] AS [FK_VALUE] FROM CTE{2} WHERE [{3}] = 0x08  AND [{0}] > 0x00000000 AND [{1}] > 0x00000000000000000000000000000000",
                        type_code_name, value_name, table_name, locator_name));
                }
            }

            return query.ToString();
        }
        private MetadataTreeNode GetParent(MetadataTreeNode node)
        {
            MetadataTreeNode root = node;
            while (root.Parent != null)
            {
                root = (MetadataTreeNode)root.Parent;
            }
            return root;
        }
        private string BuildParentEntityKeysTableQueryText(MetadataTreeNode node)
        {
            StringBuilder queryText = new StringBuilder();
            queryText.Append("DECLARE @KeysListXML nvarchar(max) = '<list>");
            foreach (Guid key in node.Keys)
            {
                queryText.AppendFormat("<item key=\"{0}\"/>", key.ToString());
            }
            queryText.AppendLine("</list>';");
            queryText.AppendLine("DECLARE @xml xml = CAST(@KeysListXML AS xml);");
            queryText.AppendLine("DECLARE @KeysListTable TABLE([key] binary(16) PRIMARY KEY);");
            queryText.AppendLine("INSERT @KeysListTable([key])");
            queryText.AppendLine("SELECT");
            queryText.AppendLine("list.[item].value('@key', 'uniqueidentifier')");
            queryText.AppendLine("FROM @xml.nodes('list/item') AS list([item]);");
            return queryText.ToString();
        }
        private string BuildSelectEntityQueryText(Entity entity, Property property)
        {
            string table_name = entity.MainTable.Name;

            string where_filter = "";
            if (property.Fields.Count == 1)
            {
                where_filter = string.Format("S.[{0}] = T.[key]", property.Fields[0].Name);
            }
            else
            {
                string value_name = "";
                string locator_name = "";
                string type_code_name = "";
                int type_code = entity.Code;
                foreach (IFieldInfo field in property.Fields)
                {
                    switch (field.Purpose)
                    {
                        case FieldPurpose.Object: { value_name = field.Name; break; }
                        case FieldPurpose.Locator: { locator_name = field.Name; break; }
                        case FieldPurpose.TypeCode: { type_code_name = field.Name; break; }
                    }
                }
                if (locator_name == string.Empty)
                {
                    where_filter = string.Format("S.[{0}] = CAST({1} AS binary(4)) AND S.[{2}] = T.[key]",
                        type_code_name, type_code, value_name);
                }
                else
                {
                    where_filter = string.Format("S.[{0}] = 0x08 AND S.[{1}] = CAST({2} AS binary(4)) AND S.[{3}] = T.[key]",
                        locator_name, type_code_name, type_code, value_name);
                }
            }

            string select_statement = string.Format("SELECT {fields_to_select} FROM [{0}] AS S INNER JOIN @KeysListTable AS T ON {1};", table_name, where_filter);

            return select_statement;
        }
    }
}

// 0x00000000000000000000000000000000
// '00000000-0000-0000-0000-000000000000'

// SELECT [key] INTO #ParentKeysTable FROM [parent table used to filter child tables];
// CREATE CLUSTERED INDEX cx ON #ParentKeysTable([key]);
// SELECT * FROM #ParentKeysTable;

//DECLARE @ParentKeysTable TABLE([key] uniqueidentifier PRIMARY KEY);
//INSERT @ParentKeysTable([key]) VALUES('00000000-0000-0000-0000-0000FFFFFFF2'), ('00000000-0000-0000-0000-0000FFFFFFF3');

//WITH CTE(fields to look up foreign keys from child table) AS
//(
//    SELECT [fk properties referencing some outer entity] FROM [child_table] AS C INNER JOIN @ParentKeysTable AS P ON C.[fk child property 1] = P.[key] -- select for each fk property
//    UNION (?) ALL -- instead of UNION can be WHERE with OR syntax used ...
//    SELECT [fk properties referencing some outer entity] FROM [child_table] AS C INNER JOIN @ParentKeysTable AS P ON C.[fk child property 2] = P.[key] -- select for each fk property
//)
//SELECT F1 AS TYPE_CODE, F2 AS FK_VALUE FROM CTE WHERE F0 = 0x08 AND F1 > 0 AND F2 > 0
//UNION ALL
//SELECT F3 AS TYPE_CODE, F4 AS FK_VALUE FROM CTE;