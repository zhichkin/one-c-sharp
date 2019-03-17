using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Zhichkin.Hermes.Infrastructure;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.Services;
using Zhichkin.ORM;

//System.Dynamic.Runtime

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
        private string GetErrorText(Exception ex)
        {
            string errorText = string.Empty;
            Exception error = ex;
            while (error != null)
            {
                errorText += (errorText == string.Empty) ? error.Message : Environment.NewLine + error.Message;
                error = error.InnerException;
            }
            return errorText;
        }
        public Dictionary<string, object> Parameters { get; } = new Dictionary<string, object>();
        
        public async Task BuildDocumentsTree(IEntityInfo document, DateTime period, Action<string> notifyStateCallback, Action<MetadataTreeNode> workIsDoneCallback)
        {
            if (document == null) throw new ArgumentNullException("document");

            this.Parameters.Clear();
            this.Parameters.Add("Period", period);

            MetadataTreeNode root = new MetadataTreeNode()
            {
                Name = document.Name,
                MetadataInfo = document
            };

            string message = "Start: " + DateTime.Now.ToUniversalTime();
            WriteToLog(message);
            notifyStateCallback(message);
            await FillChildren(root, notifyStateCallback);
            message = "End: " + DateTime.Now.ToUniversalTime();
            WriteToLog(message);
            notifyStateCallback(message);
            notifyStateCallback("");

            RemoveZeroCountNodes(root);
            workIsDoneCallback(root);

            try
            {
                CreateExchangeTable();
                //await RegisterEntityReferencesForExchange(root); !!!
                await RegisterForeignReferencesForExchange(root);
                List<dynamic> list = SelectExchangeInfo((InfoBase)document.Namespace.InfoBase);
                foreach (dynamic item in list)
                {
                    MetadataTreeNode node = new MetadataTreeNode()
                    {
                        Name = item.Entity.Name,
                        Count = item.Count,
                        MetadataInfo = item.Entity
                    };
                    workIsDoneCallback(node);
                }
            }
            catch (Exception ex)
            {
                WriteToLog(GetErrorText(ex));
            }
        }
        private void RemoveZeroCountNodes(IMetadataTreeNode root)
        {
            int index = 0;
            while (index < root.Children.Count)
            {
                IMetadataTreeNode node = root.Children[index];
                if (node.Count == 0)
                {
                    root.Children.RemoveAt(index);
                }
                else
                {
                    RemoveZeroCountNodes(node);
                    index++;
                }
            }
        }

        private async Task FillChildren(MetadataTreeNode parent, Action<string> notifyStateCallback)
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
                            CountDocuments(current_entity_node, current_property);
                        }
                        else
                        {
                            ((BooleanExpression)current_nested_node.Filter).Conditions.Add(ce);
                            CountDocuments(current_nested_node, current_property);
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
        private void CountDocuments(MetadataTreeNode node, IPropertyInfo property)
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
            if (root.Keys == null) { root.Keys = GetRootKeys(root); }
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
        private List<Guid> GetRootKeys(MetadataTreeNode root)
        {
            DateTime period = (DateTime)this.Parameters["Period"];

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

        public async Task RegisterEntityReferencesForExchange(MetadataTreeNode node)
        {
            //TODO: оптимизация постоянного получения значений родительских ключей
            await Task.Delay(1);
        }

        private void CreateExchangeTable()
        {
            StringBuilder query = new StringBuilder();
            query.AppendLine(@"IF(NOT OBJECT_ID(N'[dbo].[Z_ExchangeTable]') IS NULL) DROP TABLE Z_ExchangeTable");
            query.AppendLine(@"CREATE TABLE Z_ExchangeTable(TYPE_CODE int, FK_VALUE binary(16));");
            query.AppendLine(@"CREATE CLUSTERED INDEX CX_Z_ExchangeTable ON Z_ExchangeTable(TYPE_CODE, FK_VALUE);");
            query.Append(@"CREATE INDEX IX_Z_ExchangeTable ON Z_ExchangeTable(FK_VALUE);");

            using (SqlConnection connection = new SqlConnection(connection_string))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = query.ToString();
                int rowsAffected = command.ExecuteNonQuery();
            }
        }
        private List<dynamic> SelectExchangeInfo(InfoBase infoBase)
        {
            List<dynamic> list = new List<dynamic>();

            StringBuilder query = new StringBuilder();
            query.Append("SELECT TYPE_CODE, COUNT(FK_VALUE) AS [FK_COUNT] FROM Z_ExchangeTable GROUP BY TYPE_CODE;");

            using (SqlConnection connection = new SqlConnection(connection_string))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = query.ToString();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    IPersistentContext context = MetadataPersistentContext.Current;
                    MetadataService service = new MetadataService();
                    while (reader.Read())
                    {
                        dynamic item = new ExpandoObject();
                        Entity entity = service.GetEntityInfo(infoBase, (int)reader[0]);
                        int count = (int)reader[1];
                        ((IDictionary<string, object>)item).Add("Entity", entity);
                        ((IDictionary<string, object>)item).Add("Count", count);
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Процедура отбирает объекты узла данных,
        /// по настроенному для него фильтру,
        /// а затем регистрирует внешние ссылки его свойств для обмена.
        /// Важно: свойства, используемые для фильтрации,
        /// не используются для поиска внешних объектов!
        /// </summary>
        /// <param name="node">
        /// Узел данных, для объектов которого
        /// необходимо выполнить регистрацию
        /// внешних ссылок для обмена.
        /// </param>
        /// <returns></returns>
        public async Task RegisterForeignReferencesForExchange(MetadataTreeNode node)
        {
            Entity entity = node.MetadataInfo as Entity;
            if (entity == null) { throw new ArgumentNullException("node"); }

            Entity parent = null;
            MetadataTreeNode parent_node = GetParentNode(node);
            if (parent_node != null) { parent = parent_node.MetadataInfo as Entity; }
            List<Guid> parent_keys = GetParentKeys(node);
            List<Property> filter_properties = GetFilterProperties(node);
            List<Property> foreign_references = GetForeignKeyProperties(entity, filter_properties);

            string query = BuildSelectForeignKeysScript(parent, entity, filter_properties, parent_keys, foreign_references);

            DateTime period = (DateTime)this.Parameters["Period"];
            period = new DateTime(period.Year, period.Month, period.Day, 0, 0, 0, 0);
            period = period.AddYears(2000); // fuck 1C !!!

            using (SqlConnection connection = new SqlConnection(connection_string))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = query.ToString();
                if (parent_node == null)
                {
                    command.Parameters.AddWithValue("_Date_Time", period);
                }
                int rowsAffected = command.ExecuteNonQuery();
            }

            await Task.Delay(1);
        }
        private MetadataTreeNode GetParentNode(MetadataTreeNode node)
        {
            IMetadataTreeNode parent = node.Parent;
            while (parent != null)
            {
                if (!(parent.MetadataInfo is INamespaceInfo))
                {
                    break;
                }
                parent = parent.Parent;
            }
            return (parent as MetadataTreeNode);
        }
        private List<Property> GetFilterProperties(MetadataTreeNode node)
        {
            List<Property> list = new List<Property>();

            BooleanExpression filter = node.Filter as BooleanExpression;
            if (filter == null || filter.Conditions.Count == 0)
            {
                return list;
            }

            foreach (IComparisonExpression condition in filter.Conditions)
            {
                PropertyExpression expression = condition.LeftExpression as PropertyExpression;
                list.Add((Property)expression.PropertyInfo);
            }

            return list;
        }
        private List<Property> GetForeignKeyProperties(Entity entity, List<Property> filter_properties)
        {
            List<Property> list = new List<Property>();

            foreach (Property property in entity.Properties)
            {
                //if (property.Purpose == PropertyPurpose.System) continue;
                if (property.Name == "Ссылка") continue;

                bool isFilterProperty = false;
                foreach (Property filter in filter_properties)
                {
                    if (property == filter)
                    {
                        isFilterProperty = true;
                        break;
                    }
                }
                if (isFilterProperty) continue;

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

        private List<Guid> GetParentKeys(MetadataTreeNode node)
        {
            if (node == null) { throw new ArgumentNullException("node"); }

            MetadataTreeNode parent = GetParentNode(node);
            if (parent == null) { return null; }
            if (parent.Keys != null) { return parent.Keys; }
            if (parent.Parent == null)
            {
                parent.Keys = GetRootKeys(parent);
                return parent.Keys;
            }

            List<Guid> parent_keys = null;
            while (parent_keys == null)
            {
                parent_keys = GetParentKeys(parent);
            }

            List<Guid> keys = new List<Guid>();

            Entity entity = parent.MetadataInfo as Entity;
            Entity source = node.MetadataInfo as Entity;
            List<Property> filters = GetFilterProperties(node);
            
            StringBuilder query = new StringBuilder();
            query.AppendLine(BuildKeysTableQueryScript(parent_keys));
            query.Append(BuildSelectParentKeysScript(source, filters, entity));

            using (SqlConnection connection = new SqlConnection(connection_string))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = query.ToString();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        keys.Add(new Guid((byte[])reader[0]));
                    }
                }
            }

            return keys;
        }
        private string BuildKeysTableQueryScript(List<Guid> keys)
        {
            StringBuilder script = new StringBuilder();
            script.Append("DECLARE @KeysListXML nvarchar(max) = '<list>");
            foreach (Guid key in keys)
            {
                script.AppendFormat("<item key=\"{0}\"/>", key.ToString());
            }
            script.AppendLine("</list>';");
            script.AppendLine("DECLARE @xml xml = CAST(@KeysListXML AS xml);");
            script.AppendLine("DECLARE @KeysTable TABLE([key] binary(16) PRIMARY KEY);");
            script.AppendLine("INSERT @KeysTable([key])");
            script.AppendLine("SELECT");
            script.AppendLine("list.[item].value('@key', 'uniqueidentifier')");
            script.AppendLine("FROM @xml.nodes('list/item') AS list([item]);");
            return script.ToString();
        }
        private string BuildSelectParentKeysScript(Entity source, List<Property> filters, Entity parent)
        {
            StringBuilder script = new StringBuilder();
            for (int i = 0; i < filters.Count; i++)
            {
                script.AppendLine(BuildSelectParentKeysForOnePropertyScript(source, filters[i], parent));
                if (filters.Count > 1 && i < filters.Count - 1)
                {
                    script.AppendLine("UNION");
                }
            }
            script.Append(";");
            return script.ToString();
        }
        private string BuildSelectParentKeysForOnePropertyScript(Entity source, Property filter, Entity parent)
        {
            StringBuilder script = new StringBuilder();
            script.Append("SELECT [_IDRRef] FROM [");
            script.Append(source.MainTable.Name);
            script.Append("] AS T INNER JOIN @KeysTable AS K ON ");
            script.Append(BuildFilterQueryScript(filter, "T", parent.Code));
            script.Append(";");
            return script.ToString();
        }
        private string BuildFilterQueryScript(Property property, string tableAlias, int typeCode)
        {
            StringBuilder script = new StringBuilder();

            if (property.Fields.Count == 1)
            {
                script.Append(string.Format("{0}.[{1}] = K.[key]", tableAlias, property.Fields[0].Name));
            }
            else
            {
                string object_field = string.Empty;
                string locator_field = string.Empty;
                string type_code_field = string.Empty;
                foreach (IFieldInfo field in property.Fields)
                {
                    switch (field.Purpose)
                    {
                        case FieldPurpose.Object: { object_field = field.Name; break; }
                        case FieldPurpose.Locator: { locator_field = field.Name; break; }
                        case FieldPurpose.TypeCode: { type_code_field = field.Name; break; }
                    }
                }
                if (locator_field == string.Empty)
                {
                    script.Append(string.Format(
                        "{0}.[{1}] = CAST({2} AS binary(4)) AND {0}.[{3}] = K.[key]",
                        tableAlias, type_code_field, typeCode, object_field));
                }
                else
                {
                    script.Append(string.Format(
                        "{0}.[{1}] = 0x08 AND {0}.[{2}] = CAST({3} AS binary(4)) AND {0}.[{4}] = K.[key]",
                        tableAlias, locator_field, type_code_field, typeCode, object_field));
                }
            }

            return script.ToString();
        }

        private string BuildSelectForeignKeysScript(Entity parent, Entity source, List<Property> filters, List<Guid> keys, List<Property> foreiners)
        {
            if (parent == null)
            {
                return SelectForeignKeysFromRootScript(source, foreiners);
            }
            else
            {
                return SelectForeignKeysFromChildScript(parent, source, filters, keys, foreiners);
            }
        }
        private string SelectForeignKeysFromRootScript(Entity source, List<Property> foreiners)
        {
            StringBuilder script = new StringBuilder();
            string fields = SelectFieldsScript(foreiners);
            script.AppendLine(string.Format("WITH CTE ({0}) AS", fields));
            script.AppendLine("(");
            script.AppendLine(string.Format("SELECT {0} FROM [{1}] WHERE [_Date_Time] >= @_Date_Time",
                fields, source.MainTable.Name));
            script.AppendLine(")");
            //script.Append(SelectForeignKeysFromEntityScript(foreiners));
            script.Append(MergeForeignKeysFromEntityToExchangeTableScript(foreiners));
            script.Append(";");
            return script.ToString();
        }
        private string SelectFieldsScript(List<Property> properties)
        {
            StringBuilder script = new StringBuilder();
            foreach (Property property in properties)
            {
                if (script.Length > 0) { script.Append(", "); }
                script.Append(FieldsForOnePropertyScript(property));
            }
            return script.ToString();
        }
        private string FieldsForOnePropertyScript(Property property)
        {
            StringBuilder script = new StringBuilder();
            if (property.Fields.Count == 1)
            {
                script.Append("[");
                script.Append(property.Fields[0].Name);
                script.Append("]");
            }
            else
            {
                foreach (Field field in property.Fields)
                {
                    if (field.Purpose == FieldPurpose.Object || field.Purpose == FieldPurpose.Locator || field.Purpose == FieldPurpose.TypeCode)
                    {
                        if (script.Length > 0) { script.Append(", "); }
                        script.Append("[");
                        script.Append(field.Name);
                        script.Append("]");
                    }
                }
            }
            return script.ToString();
        }
        private string SelectForeignKeysFromEntityScript(List<Property> foreiners)
        {
            StringBuilder script = new StringBuilder();
            for (int i = 0; i < foreiners.Count; i++)
            {
                script.AppendLine(SelectForeignKeysForOnePropertyScript(foreiners[i]));
                if (foreiners.Count > 1 && i < foreiners.Count - 1)
                {
                    script.AppendLine("UNION");
                }
            }
            return script.ToString();
        }
        private string SelectForeignKeysForOnePropertyScript(Property property)
        {
            StringBuilder script = new StringBuilder();

            if (property.Fields.Count == 1)
            {
                script.Append("SELECT ");
                script.Append(property.Relations[0].Entity.Code.ToString());
                script.Append(" AS [TYPE_CODE], [");
                script.Append(property.Fields[0].Name);
                script.Append("] AS [FK_VALUE] FROM CTE WHERE [");
                script.Append(property.Fields[0].Name);
                script.Append("] > 0x00000000000000000000000000000000");
            }
            else
            {
                string object_field = string.Empty;
                string locator_field = string.Empty;
                string type_code_field = string.Empty;
                foreach (Field field in property.Fields)
                {
                    switch (field.Purpose)
                    {
                        case FieldPurpose.Object: { object_field = field.Name; break; }
                        case FieldPurpose.Locator: { locator_field = field.Name; break; }
                        case FieldPurpose.TypeCode: { type_code_field = field.Name; break; }
                    }
                }
                script.Append("SELECT CAST([");
                script.Append(type_code_field);
                script.Append("] AS int) AS [TYPE_CODE], [");
                script.Append(object_field);
                script.Append("] AS [FK_VALUE] FROM CTE WHERE [");
                script.Append(type_code_field);
                script.Append("] > 0x00000000 AND [");
                script.Append(object_field);
                script.Append("] > 0x00000000000000000000000000000000");
                if (locator_field != string.Empty)
                {
                    script.Append(" AND [");
                    script.Append(locator_field);
                    script.Append("] = 0x08");
                }
            }

            return script.ToString();
        }
        private string SelectForeignKeysFromChildScript(Entity parent, Entity source, List<Property> filters, List<Guid> keys, List<Property> foreiners)
        {
            if (parent == null) throw new ArgumentNullException("parent");

            StringBuilder script = new StringBuilder();
            script.AppendLine(BuildKeysTableQueryScript(keys));
            script.AppendLine("WITH CTE (");
            script.Append(SelectFieldsScript(foreiners));
            script.AppendLine(") AS");
            script.AppendLine("(");
            script.AppendLine(SelectRecordsFromChildUsingFilters(parent, source, filters, foreiners));
            script.AppendLine(")");
            //script.Append(SelectForeignKeysFromEntityScript(foreiners));
            script.Append(MergeForeignKeysFromEntityToExchangeTableScript(foreiners));
            script.Append(";");
            return script.ToString();
        }
        private string SelectRecordsFromChildUsingFilters(Entity parent, Entity source, List<Property> filters, List<Property> foreiners)
        {
            StringBuilder script = new StringBuilder();
            for (int i = 0; i < filters.Count; i++)
            {
                script.AppendLine(SelectRecordsFromChildUsingOneFilter(parent, source, filters[i], foreiners));
                if (filters.Count > 1 && i < filters.Count - 1)
                {
                    script.AppendLine("UNION ALL");
                }
            }
            return script.ToString();
        }
        private string SelectRecordsFromChildUsingOneFilter(Entity parent, Entity source, Property filter, List<Property> foreiners)
        {
            StringBuilder script = new StringBuilder();
            script.Append("SELECT ");
            script.Append(SelectFieldsScript(foreiners));
            script.Append(" FROM [");
            script.Append(source.MainTable.Name);
            script.Append("] AS C INNER JOIN @KeysTable AS K ON ");
            script.Append(BuildFilterQueryScript(filter, "C", parent.Code));
            return script.ToString();
        }
        private string MergeForeignKeysFromEntityToExchangeTableScript(List<Property> foreiners)
        {
            StringBuilder script = new StringBuilder();
            script.AppendLine("MERGE Z_ExchangeTable AS target");
            script.AppendLine("USING");
            script.AppendLine("(");
            script.AppendLine(SelectForeignKeysFromEntityScript(foreiners));
            script.AppendLine(") AS source(TYPE_CODE, FK_VALUE)");
            script.AppendLine("ON (target.TYPE_CODE = source.TYPE_CODE AND target.FK_VALUE = source.FK_VALUE)");
            script.AppendLine("WHEN NOT MATCHED THEN");
            script.AppendLine("INSERT (TYPE_CODE, FK_VALUE) VALUES (source.TYPE_CODE, source.FK_VALUE)");
            return script.ToString();
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