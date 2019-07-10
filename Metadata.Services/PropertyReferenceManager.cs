using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Metadata.Services
{
    public sealed class PropertyReferenceManager
    {
        private readonly PropertyReference property;
        private readonly MetadataService metadata = new MetadataService();

        private Dictionary<int, string> ordinals = new Dictionary<int, string>();
        private Dictionary<FieldPurpose, int> purposes = new Dictionary<FieldPurpose, int>();

        public PropertyReferenceManager(PropertyReference property)
        {
            this.property = property;
        }
        public void Prepare(ref int currentOrdinal) // start ordinal for the table fields in SELECT clause - move to constructor ?
        {
            bool isMultiValued = (property.Property.Fields.Count > 1);

            foreach (Field field in property.Property.Fields)
            {
                string name = string.Empty;
                if (currentOrdinal > 0) { name += $"\n\t,"; }

                name += $"[{property.Table.Alias}].[{field.Name}] AS ";
                if (isMultiValued)
                {
                    name += $"[{property.Name}_{GetFieldPurposeSuffix(field)}]";
                }
                else
                {
                    name += $"[{property.Name}]";
                }
                ordinals.Add(currentOrdinal, name);
                purposes.Add(field.Purpose, currentOrdinal);
                currentOrdinal++;
            }
        }
        public string ToSQL()
        {
            string sql = "";
            foreach (KeyValuePair<int, string> item in ordinals)
            {
                sql += item.Value;
            }
            return sql;
        }

        private string GetFieldPurposeSuffix(Field field)
        {
            if (field.Purpose == FieldPurpose.Locator)
            {
                return "TYPE";
            }
            else if (field.Purpose == FieldPurpose.TypeCode)
            {
                return "T";
            }
            else if (field.Purpose == FieldPurpose.Value)
            {
                return string.Empty;
            }
            else if (field.Purpose == FieldPurpose.Object)
            {
                return "R";
            }
            else if (field.Purpose == FieldPurpose.Boolean)
            {
                return "L";
            }
            else if (field.Purpose == FieldPurpose.Number)
            {
                return "N";
            }
            else if (field.Purpose == FieldPurpose.String)
            {
                return "S";
            }
            else if (field.Purpose == FieldPurpose.Binary)
            {
                return "B";
            }
            else if (field.Purpose == FieldPurpose.DateTime)
            {
                return "D";
            }
            else
            {
                return string.Empty;
            }
        }

        public object GetValue(IDataReader reader)
        {
            if (property.Property.Fields.Count == 1)
            {
                return TranslateSingleType(reader);
            }
            else
            {
                return TranslateComplexType(reader);
            }
        }
        private object TranslateSingleType(IDataReader reader)
        {
            Field field = property.Property.Fields[0];
            int index = purposes[field.Purpose]; // FieldPurpose.Value || FieldPurpose.Object

            object value = reader[index];
            if (value == DBNull.Value) return Utilities.GetDefaultValueAsObject(field);

            if (field.Purpose == FieldPurpose.Object)
            {
                Entity entity = property.Property.Relations[0].Entity;

                Guid identity = Guid.Empty;
                if (value is Guid)
                {
                    identity = (Guid)value;
                }
                else
                {
                    identity = new Guid((byte[])value);
                }
                
                return new ReferenceProxy(entity, identity);
            }

            return value;
        }
        private object TranslateComplexType(IDataReader reader)
        {
            byte[] buffer;
            object value;
            
            int typeCode = -1;
            if (purposes.TryGetValue(FieldPurpose.TypeCode, out typeCode))
            {
                value = reader[typeCode];
                if (value is int)
                {
                    typeCode = (int)value;
                }
                else
                {
                    buffer = (byte[])value;
                    typeCode = BitConverter.ToInt32(buffer, 0);
                }
            }

            if (typeCode > 0) // ReferenceObject
            {
                Relation relation = property.Property.Relations.Where(r => r.Entity.Code == typeCode).FirstOrDefault();
                if (relation == null) return null; // unsupported value type for this property
                
                int index = -1;
                if (!purposes.TryGetValue(FieldPurpose.Object, out index))
                {
                    index = purposes[FieldPurpose.Value];
                }

                Guid identity = Guid.Empty;
                value = reader[index];
                if (value is Guid)
                {
                    identity = (Guid)value;
                }
                else
                {
                    buffer = (byte[])reader[index];
                    identity = new Guid(buffer);
                }

                return new ReferenceProxy(relation.Entity, identity);
            }

            // primitive types

            int locator = purposes[FieldPurpose.Locator];
            buffer = (byte[])reader[locator];
            locator = buffer[0];

            value = null;
            if (locator == 1) // Неопределено
            {
                return null;
            }
            else if (locator == 2) // Булево
            {
                value = ((byte[])reader[purposes[FieldPurpose.Boolean]])[0] == 0 ? false : true;
            }
            else if (locator == 3) // Число
            {
                value = (decimal)reader[purposes[FieldPurpose.Number]];
            }
            else if (locator == 4) // Дата
            {
                value = reader.GetDateTime(purposes[FieldPurpose.DateTime]);
            }
            else if (locator == 5) // Строка
            {
                value = (string)reader[purposes[FieldPurpose.String]];
            }
            else if (locator == 8) // Ссылка
            {
                Relation relation = property.Property.Relations.Where(r => r.Entity.Code == typeCode).FirstOrDefault();
                if (relation == null) return null; // unsupported value type for this property

                buffer = (byte[])reader[purposes[FieldPurpose.Object]];
                Guid identity = new Guid(buffer);
                value = new ReferenceProxy(relation.Entity, identity);
            }
            else
            {
                return null;
            }
            return value;
        }
    }
}
