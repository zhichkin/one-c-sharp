using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Zhichkin.Hermes.Infrastructure;
using Zhichkin.Metadata.Model;
using Zhichkin.ORM;

namespace Zhichkin.Hermes.Services
{
    public sealed class DocumentsTreeService
    {
        public DocumentsTreeService() { }

        public IMetadataTreeNode BuildDocumentsTree(IEntityInfo document)
        {
            if (document == null)
            {
                IPersistentContext context = MetadataPersistentContext.Current;
                document = context.Factory.New<Entity>(new Guid("3DB7B64A-382E-4C64-9044-8F7EF0CF741C"));
            }

            MetadataTreeNode root = new MetadataTreeNode()
            {
                Name = document.Name,
                MetadataInfo = document
            };
            
            FillChildren(root);

            return root;
        }
        private void FillChildren(MetadataTreeNode parent)
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
                    }
                }
            }
        }
        private void CountDocuments(MetadataTreeNode node, IPropertyInfo property)
        {
            string table_name = ((Entity)property.Entity).MainTable.Name;
            IList<IFieldInfo> fields = property.Fields;
            node.Name = string.Format("{0} ({1})", node.Name, fields.Count.ToString());
        }
    }
}
