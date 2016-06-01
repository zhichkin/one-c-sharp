using System;
using Zhichkin.Metadata.Model;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Zhichkin.Metadata.Services
{
    public interface IMetadataAdapter
    {
        void Load(string connectionString, InfoBase infoBase);
    }

    public sealed class COMMetadataAdapter : IMetadataAdapter
    {
        private Dictionary<string, Namespace> ReferenceObjectsNamespaces = new Dictionary<string, Namespace>();
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
            }
        }

        private void InitializeDictionaries(InfoBase infoBase)
        {
            Namespace ns = new Namespace() { Owner = infoBase, Name = "Справочники" };
            ReferenceObjectsNamespaces.Add(ns.Name, ns); infoBase.Namespaces.Add(ns);
            ns = new Namespace() { Owner = infoBase, Name = "Документы" };
            ReferenceObjectsNamespaces.Add(ns.Name, ns); infoBase.Namespaces.Add(ns);
            ns = new Namespace() { Owner = infoBase, Name = "ПланыВидовХарактеристик" };
            ReferenceObjectsNamespaces.Add(ns.Name, ns); infoBase.Namespaces.Add(ns);
            ns = new Namespace() { Owner = infoBase, Name = "ПланыОбмена" };
            ReferenceObjectsNamespaces.Add(ns.Name, ns); infoBase.Namespaces.Add(ns);
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
                        //row.GetAndWrap("Поля")
                        //ИмяПоляХранения(StorageFieldName)
                        //ИмяПоля(FieldName) 
                        if (table.Purpose == TablePurpose.Main)
                        {
                            entity.Code = GetTypeCode(table.Name);
                        }
                        entity.Tables.Add(table);
                    }
                }
            }
        }
        private void LoadAttributesTypeInfo()
        {
            //    Процедура ОпределитьТипыСвойства(ОбъектМетаданных, Свойство, КодыТипов)

            //    ОписаниеТипов = ОбъектМетаданных.Тип;
            //    Типы = ОписаниеТипов.Типы();

            //    Для Каждого Тип Из Типы Цикл

            //        ИмяТипа = "unknown";

            //        Если Строка(Тип) = "Булево" Тогда
            //            ИмяТипа = "bit";
            //        ИначеЕсли Строка(Тип) = "Число" Тогда
            //            ИмяТипа = ПолучитьОписаниеЧисла(ОписаниеТипов);
            //        ИначеЕсли Строка(Тип) = "Строка" Тогда
            //            ИмяТипа = ПолучитьОписаниеСтроки(ОписаниеТипов);
            //        ИначеЕсли Строка(Тип) = "Дата" Тогда
            //            ИмяТипа = ПолучитьОписаниеДаты(ОписаниеТипов);
            //        ИначеЕсли Строка(Тип) = "Хранилище значения" Тогда
            //            ИмяТипа = ПолучитьОписаниеДвоичныхДанных(ОписаниеТипов);
            //        ИначеЕсли Строка(Тип) = "Уникальный идентификатор" Тогда
            //            ИмяТипа = "uniqueidentifier";
            //        ИначеЕсли Строка(Тип) = "ВидДвиженияНакопления" Тогда
            //            ИмяТипа = "uint(1)";
            //        Иначе
            //            Попытка
            //                Ссылка = Новый(Тип);
            //                ИмяТипа = Ссылка.Метаданные().ПолноеИмя();
            //                Поиск = КодыТипов.Найти(ИмяТипа, "Имя");
            //                Если Поиск <> Неопределено Тогда
            //                    ИмяТипа = Формат(Поиск.Код, "ЧН=; ЧГ=0");
            //                КонецЕсли;
            //            Исключение
            //                // unknown
            //                Сообщить("Не удалось определить тип: " + Строка(Тип));
            //            КонецПопытки;
            //        КонецЕсли;

            //        Свойство.Типы.Добавить(ИмяТипа);

            //    КонецЦикла;

            //КонецПроцедуры
        }
    }
}
