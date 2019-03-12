using System;
using Zhichkin.Metadata.Model;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Zhichkin.Hermes.Infrastructure;

namespace Zhichkin.Metadata.Services
{
    public sealed class COMMetadataAdapter : IMetadataAdapter
    {
        private Dictionary<string, Namespace> ReferenceObjectsNamespaces = new Dictionary<string, Namespace>();
        private Dictionary<string, Namespace> ReferenceNamespacesLookup = new Dictionary<string, Namespace>();
        private Dictionary<string, Namespace> ValueObjectsNamespaces = new Dictionary<string, Namespace>();
        private Dictionary<PropertyPurpose, string> PropertyPurposes = new Dictionary<PropertyPurpose, string>()
        {
            { PropertyPurpose.Property,  "Реквизиты"  },
            { PropertyPurpose.Dimension, "Измерения" },
            { PropertyPurpose.Measure,   "Ресурсы" },
            { PropertyPurpose.System,    "СтандартныеРеквизиты" }
        };
        private Dictionary<string, Table> Tables = new Dictionary<string, Table>();

        public void Load(string connectionString, InfoBase infoBase)
        {
            using (ComConnector connector = new ComConnector(connectionString))
            {
                connector.Connect();
                using (IComWrapper metadata = connector.Metadata)
                {
                    infoBase.Name = (string)metadata.Get("Имя");
                }

                InitializeDictionaries(infoBase);

                foreach (var item in ReferenceObjectsNamespaces)
                {
                    LoadReferenceMetaObjects(connector, item.Key);
                }
                foreach (var item in ValueObjectsNamespaces)
                {
                    LoadValueMetaObjects(connector, item.Key);
                }
                LoadPropertiesTypes(connector);
            }
            GC.Collect(); // !? COM-соединение подвисает в 1С
        }

        private void InitializeDictionaries(InfoBase infoBase)
        {
            Namespace ns = new Namespace() { Owner = infoBase, Name = "Справочники" };
            ReferenceObjectsNamespaces.Add(ns.Name, ns); infoBase.Namespaces.Add(ns);
            ReferenceNamespacesLookup.Add("CatalogRef", ns);

            ns = new Namespace() { Owner = infoBase, Name = "Документы" };
            ReferenceObjectsNamespaces.Add(ns.Name, ns); infoBase.Namespaces.Add(ns);
            ReferenceNamespacesLookup.Add("DocumentRef", ns);

            ns = new Namespace() { Owner = infoBase, Name = "ПланыВидовХарактеристик" };
            ReferenceObjectsNamespaces.Add(ns.Name, ns); infoBase.Namespaces.Add(ns);
            ReferenceNamespacesLookup.Add("ChartOfCharacteristicTypesRef", ns);

            ns = new Namespace() { Owner = infoBase, Name = "ПланыОбмена" };
            ReferenceObjectsNamespaces.Add(ns.Name, ns); infoBase.Namespaces.Add(ns);
            ReferenceNamespacesLookup.Add("ExchangePlanRef", ns);

            ns = new Namespace() { Owner = infoBase, Name = "РегистрыСведений" };
            ValueObjectsNamespaces.Add(ns.Name, ns); infoBase.Namespaces.Add(ns);

            ns = new Namespace() { Owner = infoBase, Name = "РегистрыНакопления" };
            ValueObjectsNamespaces.Add(ns.Name, ns); infoBase.Namespaces.Add(ns);
        }
        private int GetTypeCode(string tableName)
        {
            int result;
            StringBuilder numbers = new StringBuilder();
            Regex regex = new Regex("[0-9]+");
            foreach (Match m in regex.Matches(tableName))
            {
                numbers.Append(m.Value);
            }
            if (int.TryParse(numbers.ToString(), out result))
            {
                return result;
            }
            return 0;
        }
        private Entity GetEntityByMetaTypeName(string typeName)
        {
            string[] names = typeName.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);

            Namespace ns;
            if (ReferenceNamespacesLookup.TryGetValue(names[0], out ns))
            {
                return GetEntityByName(ns, names[1]);
            }
            return Entity.Empty;
        }
        private Entity GetEntityByName(Namespace _namespace, string name)
        {
            foreach (Entity entity in _namespace.Entities)
            {
                if (entity.Name == name)
                {
                    return entity;
                }
            }
            return Entity.Empty;
        }
        private Property GetPropertyByName(Entity entity, string name)
        {
            foreach (Property property in entity.Properties)
            {
                if (property.Name == name)
                {
                    return property;
                }
            }
            return null;
        }
        private Entity GetNestedEntityByName(Entity entity, string name)
        {
            foreach (Entity nested in entity.NestedEntities)
            {
                if (nested.Name == name)
                {
                    return nested;
                }
            }
            return null;
        }
        private bool IsReferenceEntity(string tableName)
        {
            StringComparison option = StringComparison.InvariantCultureIgnoreCase;
            return tableName.StartsWith("_Reference", option)
                || tableName.StartsWith("_Document", option)
                || tableName.StartsWith("_Node", option)
                || tableName.StartsWith("_Task", option)
                || tableName.StartsWith("_Chrc", option)
                || tableName.StartsWith("_Acc", option);
        }
        private bool IsReferenceEntity(Entity entity)
        {
            return ReferenceObjectsNamespaces.ContainsKey(entity.Namespace.Name);
        }
        private void LoadStandardAttributes(IComWrapper metaObject, Entity entity)
        {
            using (IComWrapper attributes = metaObject.GetAndWrap("СтандартныеРеквизиты"))
            {
                IEnumerable iterator = (IEnumerable)attributes.ComObject;
                foreach (object item in iterator)
                {
                    using (IComWrapper attribute = attributes.Wrap(item))
                    {
                        Property property = new Property()
                        {
                            Entity = entity,
                            Name = (string)attribute.Get("Имя"),
                            Purpose = PropertyPurpose.System
                        };
                        entity.Properties.Add(property);
                    }
                }
                attributes.Release(ref iterator);
            }
        }
        private void LoadAttributes(IComWrapper metaObject, Entity entity, PropertyPurpose purpose)
        {
            using (IComWrapper attributes = metaObject.GetAndWrap(PropertyPurposes[purpose]))
            {
                int count = (int)attributes.Call("Количество");
                for (int i = 0; i < count; i++)
                {
                    using (IComWrapper attribute = attributes.CallAndWrap("Получить", i))
                    {
                        Property property = new Property()
                        {
                            Entity = entity,
                            Name = (string)attribute.Get("Имя"),
                            Purpose = purpose
                        };
                        entity.Properties.Add(property);
                    }
                }
            }
        }
        private void LoadPropertiesTypes(ComConnector connector)
        {
            foreach (var item in ReferenceObjectsNamespaces)
            {
                LoadEntityProperties(connector, item.Value,
                    new PropertyPurpose[]
                    {
                        PropertyPurpose.Property
                    });
            }
            foreach (var item in ValueObjectsNamespaces)
            {
                LoadEntityProperties(connector, item.Value,
                    new PropertyPurpose[]
                    {
                        PropertyPurpose.Dimension,
                        PropertyPurpose.Measure,
                        PropertyPurpose.Property
                    });
            }
        }
        private void LoadEntityProperties(ComConnector connector, Namespace _namespace, PropertyPurpose[] purposes)
        {
            using (IComWrapper metadata = connector.Metadata)
            using (IComWrapper metaObjects = metadata.GetAndWrap(_namespace.Name))
            {
                int count = (int)metaObjects.Call("Количество");
                for (int i = 0; i < count; i++)
                {
                    using (IComWrapper metaObject = metaObjects.CallAndWrap("Получить", i))
                    {
                        Entity entity = GetEntityByName(_namespace, (string)metaObject.Get("Имя"));
                        if (entity == Entity.Empty) continue;
                        LoadAttributesTypeInfo(connector, metaObject, entity, purposes);
                        if (IsReferenceEntity(entity))
                        {
                            LoadAttributesTypesForTableParts(connector, metaObject, entity);
                        }
                    }
                }
            }
        }
        private void LoadAttributesTypesForTableParts(ComConnector connector, IComWrapper metaObject, Entity entity)
        {
            using (IComWrapper tableParts = metaObject.GetAndWrap("ТабличныеЧасти"))
            {
                int count = (int)tableParts.Call("Количество");
                for (int i = 0; i < count; i++)
                {
                    using (IComWrapper tablePart = tableParts.CallAndWrap("Получить", i))
                    {
                        Entity nested = GetNestedEntityByName(entity, (string)tablePart.Get("Имя"));
                        if (nested == null) continue;
                        LoadAttributesTypeInfo(connector, tablePart, nested, new PropertyPurpose[] { PropertyPurpose.Property });
                    }
                }
            }
        }
        private void LoadAttributesTypeInfo(ComConnector connector, IComWrapper metaObject, Entity entity, PropertyPurpose[] purposes)
        {
            using (IComWrapper attributes = metaObject.GetAndWrap("СтандартныеРеквизиты"))
            {
                IEnumerable iterator = (IEnumerable)attributes.ComObject;
                foreach (object item in iterator)
                {
                    using (IComWrapper attribute = attributes.Wrap(item))
                    {
                        Property property = GetPropertyByName(entity, (string)attribute.Get("Имя"));
                        if (property == null) continue;
                        LoadAttributeTypeInfo(connector, attribute, property);
                    }
                }
                attributes.Release(ref iterator);
            }
            foreach(PropertyPurpose purpose in purposes)
            {
                using (IComWrapper attributes = metaObject.GetAndWrap(PropertyPurposes[purpose]))
                {
                    int count = (int)attributes.Call("Количество");
                    for (int i = 0; i < count; i++)
                    {
                        using (IComWrapper attribute = attributes.CallAndWrap("Получить", i))
                        {
                            Property property = GetPropertyByName(entity, (string)attribute.Get("Имя"));
                            if (property == null) continue;
                            LoadAttributeTypeInfo(connector, attribute, property);
                        }
                    }
                }
            }
        }
        private void LoadAttributeTypeInfo(ComConnector connector, IComWrapper metaProperty, Property property)
        {
            using (IComWrapper typeInfo = metaProperty.GetAndWrap("Тип"))
            using (IComWrapper types = typeInfo.CallAndWrap("Типы"))
            {
                int count = (int)types.Call("Количество");
                for (int i = 0; i < count; i++)
                {
                    using (IComWrapper type = types.CallAndWrap("Получить", i))
                    {
                        string typeName = connector.GetTypeName(type.ComObject);
                        if ("boolean" == typeName)
                        {
                            property.Relations.Add(new Relation() { Entity = Entity.Boolean, Property = property });
                        }
                        else if ("decimal" == typeName)
                        {
                            property.Relations.Add(new Relation() { Entity = Entity.Decimal, Property = property });
                        }
                        else if ("numeric" == typeName)
                        {
                            LoadNumberType(connector, typeInfo, property);
                        }
                        else if ("string" == typeName)
                        {
                            property.Relations.Add(new Relation() { Entity = Entity.String, Property = property });
                        }
                        else if ("dateTime" == typeName)
                        {
                            property.Relations.Add(new Relation() { Entity = Entity.DateTime, Property = property });
                        }
                        else if ("ValueStorage" == typeName)
                        {
                            property.Relations.Add(new Relation() { Entity = Entity.Binary, Property = property });
                        }
                        else if ("Уникальный идентификатор" == typeName)
                        {
                            property.Relations.Add(new Relation() { Entity = Entity.GUID, Property = property });
                        }
                        else if ("ВидДвиженияНакопления" == typeName)
                        {
                            property.Relations.Add(new Relation() { Entity = Entity.Byte, Property = property });
                        }
                        else
                        {
                            Entity entity = GetEntityByMetaTypeName(typeName);
                            if (entity != Entity.Empty)
                            {
                                property.Relations.Add(new Relation() { Entity = entity, Property = property });
                            }
                            else
                            {
                                // TODO: ошибка определения типа !
                            }
                        }
                    }
                }
            }
        }
        private void LoadNumberType(ComConnector connector, IComWrapper metaObject, Property property)
        {
            using (IComWrapper numberInfo = metaObject.GetAndWrap("КвалификаторыЧисла"))
            {
                if ((int)numberInfo.Get("РазрядностьДробнойЧасти") > 0)
                {
                    property.Relations.Add(new Relation() { Entity = Entity.Decimal, Property = property });
                }
                else
                {
                    using (IComWrapper sign = numberInfo.GetAndWrap("ДопустимыйЗнак"))
                    {
                        if (connector.ToString(sign) == "Любой")
                        {
                            property.Relations.Add(new Relation() { Entity = Entity.Int32, Property = property });
                        }
                        else
                        {
                            property.Relations.Add(new Relation() { Entity = Entity.UInt32, Property = property });
                        }
                    }
                }
            }
        }
        private void LoadTableParts(IComWrapper metaObject, Entity entity)
        {
            using (IComWrapper tableParts = metaObject.GetAndWrap("ТабличныеЧасти"))
            {
                int count = (int)tableParts.Call("Количество");
                for (int i = 0; i < count; i++)
                {
                    using (IComWrapper tablePart = tableParts.CallAndWrap("Получить", i))
                    {
                        Entity newEntity = new Entity()
                        {
                            Name = (string)tablePart.Get("Имя"),
                            Owner = entity
                        };
                        LoadStandardAttributes(tablePart, newEntity);
                        LoadAttributes(tablePart, newEntity, PropertyPurpose.Property);
                        entity.NestedEntities.Add(newEntity);
                    }
                }
            }
        }
        private void LoadReferenceMetaObjects(ComConnector connector, string namespaceName)
        {
            using (IComWrapper metadata = connector.Metadata)
            using (IComWrapper metaObjects = metadata.GetAndWrap(namespaceName))
            {
                int count = (int)metaObjects.Call("Количество");
                for (int i = 0; i < count; i++)
                {
                    using (IComWrapper metaObject = metaObjects.CallAndWrap("Получить", i))
                    {
                        LoadReferenceMetaObject(connector, namespaceName, metaObject);
                    }
                }
            }
        }
        private void LoadReferenceMetaObject(ComConnector connector, string namespaceName, IComWrapper metaObject)
        {
            Entity entity = new Entity() { Name = (string)metaObject.Get("Имя") };

            entity.Namespace = ReferenceObjectsNamespaces[namespaceName];
            entity.Namespace.Entities.Add(entity);

            LoadStandardAttributes(metaObject, entity);
            LoadAttributes(metaObject, entity, PropertyPurpose.Property);
            LoadTableParts(metaObject, entity);

            LoadDataStorageInfo(connector, metaObject, entity);
        }
        private void LoadValueMetaObjects(ComConnector connector, string namespaceName)
        {
            using (IComWrapper metadata = connector.Metadata)
            using (IComWrapper metaObjects = metadata.GetAndWrap(namespaceName))
            {
                int count = (int)metaObjects.Call("Количество");
                for (int i = 0; i < count; i++)
                {
                    using (IComWrapper metaObject = metaObjects.CallAndWrap("Получить", i))
                    {
                        LoadValueMetaObject(connector, namespaceName, metaObject);
                    }
                }
            }
        }
        private void LoadValueMetaObject(ComConnector connector, string namespaceName, IComWrapper metaObject)
        {
            Entity entity = new Entity() { Name = (string)metaObject.Get("Имя") };

            entity.Namespace = ValueObjectsNamespaces[namespaceName];
            entity.Namespace.Entities.Add(entity);

            LoadStandardAttributes(metaObject, entity);
            LoadAttributes(metaObject, entity, PropertyPurpose.Dimension);
            LoadAttributes(metaObject, entity, PropertyPurpose.Measure);
            LoadAttributes(metaObject, entity, PropertyPurpose.Property);

            LoadDataStorageInfo(connector, metaObject, entity);
        }
        private void LoadDataStorageInfo(ComConnector connector, IComWrapper metaObject, Entity entity)
        {
            using (IComWrapper dbNames = connector.GetDBNames(metaObject))
            {
                int count = (int)dbNames.Call("Количество");
                for (int i = 0; i < count; i++)
                {
                    using (IComWrapper row = dbNames.CallAndWrap("Получить", i))
                    {
                        Table table = new Table()
                        {
                            Entity = entity,
                            Schema = "dbo",
                            Name = (string)row.Get("ИмяТаблицыХранения"),
                            Purpose = TablePurposes.Lookup[(string)row.Get("Назначение")]
                        };
                        if (table.Purpose == TablePurpose.Main && IsReferenceEntity(table.Name))
                        {
                            entity.Code = GetTypeCode(table.Name);
                        }
                        using (IComWrapper fields = row.GetAndWrap("Поля"))
                        {
                            LoadTableFields(connector, fields, table);
                        }
                        entity.Tables.Add(table);
                    }
                }
            }
        }
        private void LoadTableFields(ComConnector connector, IComWrapper fields, Table table)
        {
            int count = (int)fields.Call("Количество");
            for (int i = 0; i < count; i++)
            {
                using (IComWrapper item = fields.CallAndWrap("Получить", i))
                {
                    Field field = new Field()
                    {
                        Table = table,
                        Name = (string)item.Get("ИмяПоляХранения")
                    };
                    MapFieldToProperty(table.Entity, (string)item.Get("ИмяПоля"), field);
                    // TODO: определить тип данных и другие свойства поля таблицы СУБД
                    table.Fields.Add(field);
                }
            }
        }
        private void MapFieldToProperty(Entity entity, string propertyName, Field field)
        {
            Property property;

            // имя поля объекта 1С не указано - системное поле, недоступное в языке запросов 1С
            if (string.IsNullOrEmpty(propertyName))
            {
                property = new Property()
                {
                    Entity = entity,
                    Name = field.Name,
                    Purpose = PropertyPurpose.System
                };
                entity.Properties.Add(property);
                field.Property = property;
                return;
            }
            
            // поле объекта, доступное в языке запросов 1С
            for (int i = 0; i < entity.Properties.Count; i++)
            {
                property = entity.Properties[i];
                if (property.Name == propertyName)
                {
                    field.Property = property;
                    return;
                }
            }

            // такого не должно быть ...
            // поле объекта 1С указано на уровне сопоставлнеия полей таблиц СУБД и свойств объекта,
            // но его нет среди доступных свойств объекта метаданных ...
            property = new Property()
            {
                Entity = entity,
                Name = propertyName,
                Purpose = PropertyPurpose.System
            };
            entity.Properties.Add(property);
            field.Property = property;
        }
    }
}
