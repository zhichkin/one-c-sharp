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
            foreach (Field field in property.Property.Fields)
            {
                string name = $"[{property.Table.Alias}].[{field.Name}] AS [f{currentOrdinal}]";
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
                sql += (item.Key == 0) ? item.Value : ", " + item.Value;
            }
            return sql;
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
                Guid identity = new Guid((byte[])value);
                return new ReferenceProxy(entity, identity);
            }

            return value;
        }
        private object TranslateComplexType(IDataReader reader)
        {
            byte[] buffer;
            
            int typeCode = -1;
            if (purposes.TryGetValue(FieldPurpose.TypeCode, out typeCode))
            {
                buffer = (byte[])reader[typeCode];
                typeCode = BitConverter.ToInt32(buffer, 0);
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
                buffer = (byte[])reader[index];
                Guid identity = new Guid(buffer);

                return new ReferenceProxy(relation.Entity, identity);
            }

            // primitive types

            int locator = purposes[FieldPurpose.Locator];
            buffer = (byte[])reader[locator];
            locator = buffer[0];

            object value = null;

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
