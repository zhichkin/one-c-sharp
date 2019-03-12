using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Xml;
using Zhichkin.Integrator.Model;
using Zhichkin.Metadata.Model;
using Zhichkin.Hermes.Infrastructure;

namespace Zhichkin.Integrator.Services
{
    public sealed class XMLMessageProducer
    {
        private readonly IIntegratorService _service = new IntegratorService();
        private Subscription _subscription = null;
        private Dictionary<int, int> _type_codes_lookup = new Dictionary<int, int>();
        private Dictionary<string, Action<XmlWriter, SqlDataReader, Subscription>> _formatters = new Dictionary<string, Action<XmlWriter, SqlDataReader, Subscription>>();
        public XMLMessageProducer()
        {
            _formatters.Add("I", WriteInsertElement);
            _formatters.Add("U", WriteUpdateElement);
            _formatters.Add("D", WriteDeleteElement);
        }
        public XMLMessageProducer Use(Subscription subscription)
        {
            if (subscription == null) throw new ArgumentNullException("subscription");
            if (subscription.Publisher.ChangeTrackingSystem == ChangeTrackingSystem.None) throw new InvalidOperationException("Publisher does not use any change tracking system!");
            _subscription = subscription;
            _type_codes_lookup = _service.GetTypeCodesLookup(_subscription);
            return this;
        }
        private int GetCorrespondingTypeCode(int code)
        {
            int result = code;
            _type_codes_lookup.TryGetValue(code, out result);
            return result;
        }

        public void Produce(SqlDataReader reader, Stream stream)
        {
            if (_subscription == null) throw new ArgumentNullException("Call \"Use\" method first!");

            using (XmlWriter writer = XmlWriter.Create(stream))
            {
                int ordinal = GetAggregatePrimaryKeyOrdinal(reader);
                if (ordinal < 0) return;

                WriteMessageElement(writer); // Message

                // reader[0] -> SYS_CHANGE_OPERATION nchar(1) I, U, D
                WriteEntityElement(writer, _subscription.Subscriber); // Entity
                _formatters[reader.GetString(0)](writer, reader, _subscription);
                writer.WriteEndElement(); // Entity

                Guid aggregate = reader.IsDBNull(ordinal) ? Guid.Empty : new Guid((byte[])reader[ordinal]);
                if (aggregate != Guid.Empty) WriteDependentEntities(writer, aggregate);

                writer.WriteEndElement(); // Message
            }
        }
        private void WriteMessageElement(XmlWriter writer)
        {
            writer.WriteStartElement("M");
            writer.WriteAttributeString("key", _subscription.Identity.ToString());
        }
        private void WriteEntityElement(XmlWriter writer, Entity entity)
        {
            writer.WriteStartElement("E");
            writer.WriteAttributeString("key", entity.Identity.ToString());
            //writer.WriteAttributeString("table", entity.MainTable.Name);
            //StringBuilder sb = new StringBuilder();
            //foreach (Field field in entity.MainTable.Fields)
            //{
            //    if (!field.IsPrimaryKey) continue;
            //    if (sb.Length > 0) sb.Append(",");
            //    sb.Append(field.Name);
            //}
            //writer.WriteAttributeString("keys", sb.ToString());
        }

        private void WriteInsertElement(XmlWriter writer, SqlDataReader reader, Subscription subscription)
        {
            writer.WriteStartElement("I");
            WriteMappings(writer, reader, subscription);
            WriteDefaults(writer, subscription);
            writer.WriteEndElement();
        }
        private void WriteUpdateElement(XmlWriter writer, SqlDataReader reader, Subscription subscription)
        {
            writer.WriteStartElement("U");
            WriteMappings(writer, reader, subscription);
            WriteDefaults(writer, subscription);
            writer.WriteEndElement();
        }
        private void WriteDeleteElement(XmlWriter writer, SqlDataReader reader, Subscription subscription)
        {
            writer.WriteStartElement("D");
            WriteBindings(writer, reader, subscription);
            writer.WriteEndElement();
        }
        private void WriteDeleteElement(XmlWriter writer, SqlDataReader reader, Subscription subscription, AggregateItem item)
        {
            writer.WriteStartElement("D");
            WriteBindings(writer, reader, subscription, item);
            writer.WriteEndElement();
        }

        private void WriteMappings(XmlWriter writer, SqlDataReader reader, Subscription subscription)
        {
            Field field = null;
            for (int i = 0; i < reader.FieldCount; i++)
            {
                // The target import logic is: if attribute is absent - null value was sent
                if (reader.IsDBNull(i)) continue;

                if (subscription.Mappings.TryGetValue(reader.GetName(i), out field))
                {
                    if (field.Purpose == FieldPurpose.TypeCode && _type_codes_lookup.Count > 0)
                    {
                        int value = GetCorrespondingTypeCode(Utilities.GetInt32((byte[])reader[i]));
                        writer.WriteAttributeString(field.Name, Utilities.GetValueAsString(field, Utilities.GetByteArray(value)));
                    }
                    else
                    {
                        writer.WriteAttributeString(field.Name, Utilities.GetValueAsString(field, reader[i]));
                    }
                }
            }
        }
        private void WriteBindings(XmlWriter writer, SqlDataReader reader, Subscription subscription)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                Field field = subscription.Bindings.Where(f => f.Name == reader.GetName(i)).FirstOrDefault();
                if (field == null) continue;
                if (!subscription.Mappings.TryGetValue(field.Name, out field)) continue;
                if (field.Purpose == FieldPurpose.TypeCode && _type_codes_lookup.Count > 0)
                {
                    int value = GetCorrespondingTypeCode(Utilities.GetInt32((byte[])reader[i]));
                    writer.WriteAttributeString(field.Name, Utilities.GetValueAsString(field, Utilities.GetByteArray(value)));
                }
                else
                {
                    writer.WriteAttributeString(field.Name, Utilities.GetValueAsString(field, reader[i]));
                }
            }
        }
        private void WriteBindings(XmlWriter writer, SqlDataReader reader, Subscription subscription, AggregateItem item)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                Field field = item.Connector.Fields.Where(f => f.Name == reader.GetName(i)).FirstOrDefault();
                //Field field = subscription.Bindings.Where(f => f.Name == reader.GetName(i)).FirstOrDefault();
                if (field == null) continue;
                if (!subscription.Mappings.TryGetValue(field.Name, out field)) continue;
                if (field.Purpose == FieldPurpose.TypeCode && _type_codes_lookup.Count > 0)
                {
                    int value = GetCorrespondingTypeCode(Utilities.GetInt32((byte[])reader[i]));
                    writer.WriteAttributeString(field.Name, Utilities.GetValueAsString(field, Utilities.GetByteArray(value)));
                }
                else
                {
                    writer.WriteAttributeString(field.Name, Utilities.GetValueAsString(field, reader[i]));
                }
            }
        }
        private void WriteDefaults(XmlWriter writer, Subscription subscription)
        {
            foreach (Field field in subscription.Defaults)
            {
                writer.WriteAttributeString(field.Name, Utilities.GetDefaultValueAsString(field));
            }
        }

        private int GetAggregatePrimaryKeyOrdinal(SqlDataReader reader)
        {
            int ordinal = -1;
            foreach (Field field in _subscription.Publisher.Entity.MainTable.Fields)
            {
                if (field.IsPrimaryKey)
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (reader.GetName(i) == field.Name)
                        {
                            return i;
                        }
                    }
                }
            }
            return ordinal;
        }
        private void WriteDependentEntities(XmlWriter writer, Guid aggregate)
        {
            foreach (AggregateItem item in AggregateItem.Select(_subscription.Publisher.Entity))
            {
                Publisher publisher = Publisher.Select(item.Component.Identity);
                if (publisher == null) continue;

                IList<Subscription> list = Subscription.Select(publisher);
                Subscription subscription = list.Where(s => s.Subscriber.InfoBase == _subscription.Subscriber.InfoBase).FirstOrDefault();
                if (subscription == null) continue;

                CommandExecutor executor = new CommandExecutor()
                {
                    Item = item,
                    Writer = writer,
                    Context = subscription,
                    Aggregate = aggregate,
                    Action = WriteDependentEntity
                };
                _service.ExecuteNewScopeCommand(item.Aggregate.InfoBase, executor);
            }
        }
        private void WriteDependentEntity(Subscription subscription, AggregateItem item, XmlWriter writer, Guid aggregate, SqlCommand command)
        {
            PrepareSelectEntityDataCommand(subscription, item, aggregate, command);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                WriteEntityElement(writer, subscription.Subscriber); // Entity
                bool writeOnce = true;
                while (reader.Read())
                {
                    if (writeOnce)
                    {
                        WriteDeleteElement(writer, reader, subscription, item);
                        writeOnce = false;
                    }
                    WriteInsertElement(writer, reader, subscription);
                }
                writer.WriteEndElement(); // Entity
            }
        }
        private void PrepareSelectEntityDataCommand(Subscription subscription, AggregateItem item, Guid aggregate, SqlCommand command)
        {
            command.CommandType = CommandType.Text;
            command.CommandText = GetSelectEntityDataScript(subscription, item);
            command.Parameters.Clear();
            AddParameters(command, item, aggregate);
        }
        private string GetSelectEntityDataScript(Subscription subscription, AggregateItem item)
        {
            string sql = "SELECT {0} FROM [{1}] WHERE {2};";

            string table = subscription.Publisher.Entity.MainTable.Name;
            StringBuilder fields = new StringBuilder();
            StringBuilder where = new StringBuilder();

            AddFields(fields, subscription);
            AddConditions(where, item.Connector);

            sql = string.Format(sql, fields.ToString(), table, where.ToString());
            
            return sql;
        }
        private void AddFields(StringBuilder fields, Subscription subscription)
        {
            foreach (string name in subscription.Mappings.Keys)
            {
                if (fields.Length > 0) fields.Append(", ");
                fields.AppendFormat("[{0}]", name);
            }
        }
        private void AddConditions(StringBuilder where, Property connector)
        {
            foreach (Field field in connector.Fields)
            {
                if (field.Purpose == FieldPurpose.Locator)
                {
                    if (where.Length > 0) where.Append(" AND ");
                    where.AppendFormat("[{0}] = @locator", field.Name);
                }
                if (field.Purpose == FieldPurpose.TypeCode)
                {
                    if (where.Length > 0) where.Append(" AND ");
                    where.AppendFormat("[{0}] = @code", field.Name);
                }
                if (field.Purpose == FieldPurpose.Object || field.Purpose == FieldPurpose.Value)
                {
                    if (where.Length > 0) where.Append(" AND ");
                    where.AppendFormat("[{0}] = @value", field.Name);
                }
            }
        }
        private void AddParameters(SqlCommand command, AggregateItem item, Guid aggregate)
        {
            foreach (Field field in item.Connector.Fields)
            {
                if (field.Purpose == FieldPurpose.Locator)
                {
                    command.Parameters.AddWithValue("locator", new byte[] { 0x08 }); // reference
                }
                if (field.Purpose == FieldPurpose.TypeCode)
                {
                    command.Parameters.AddWithValue("code", Utilities.GetByteArray(item.Aggregate.Code));
                }
                if (field.Purpose == FieldPurpose.Object || field.Purpose == FieldPurpose.Value)
                {
                    command.Parameters.AddWithValue("value", aggregate.ToByteArray());
                }
            }
        }
    }
}
