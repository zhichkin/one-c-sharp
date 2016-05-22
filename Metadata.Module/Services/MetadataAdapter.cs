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
        private InfoBase infoBase;

        private Dictionary<string, Table> Tables = new Dictionary<string, Table>();
        private Dictionary<string, TablePurpose> TablePurposes = new Dictionary<string, TablePurpose>()
        {
            { "Основная", TablePurpose.Main },
            { "ТабличнаяЧасть", TablePurpose.TablePart },
            { "РегистрацияИзменений", TablePurpose.Changes },
            { "ИнициализированныеПредопределенныеДанныеСправочника", TablePurpose.InitializedPredefinedDataInCatalog }
        };

        private Dictionary<string, Namespace> Namespaces = new Dictionary<string, Namespace>();
        private Dictionary<string, Entity> Catalogs = new Dictionary<string, Entity>();

        public void Load(string connectionString, InfoBase infoBase)
        {
            this.infoBase = infoBase;
            using (ComConnector connector = new ComConnector(connectionString))
            {
                connector.Connect();
                using (IComWrapper metadata = connector.Metadata)
                {
                    infoBase.Name = (string)metadata.Get("Имя");
                }
                LoadCatalogs(connector);
                infoBase.Namespaces = new List<Namespace>();
                foreach (KeyValuePair<string, Namespace> item in Namespaces)
                {
                    infoBase.Namespaces.Add(item.Value);
                }
            }
        }
        private Namespace GetNamespace(string name)
        {
            Namespace ns;
            if (Namespaces.TryGetValue(name, out ns))
            {
                return ns;
            }
            ns = new Namespace() { Owner = infoBase, Name = name };
            Namespaces.Add(ns.Name, ns);
            return ns;
        }
        private TablePurpose GetTablePurpose(string purposeName)
        {
            return TablePurposes[purposeName];
        }
        private void LoadCatalogs(ComConnector connector)
        {
            using (IComWrapper metadata = connector.Metadata)
            using (IComWrapper catalogs = metadata.GetAndWrap("Справочники"))
            {
                int count = (int)catalogs.Call("Количество");
                for (int i = 0; i < count; i++)
                {
                    using (IComWrapper catalog = catalogs.CallAndWrap("Получить", i))
                    {
                        using (IComWrapper names = connector.GetDBNames(catalog))
                        {
                            LoadCatalog(connector, catalog, names);
                        }
                    }
                }
            }
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
        private void LoadAttributes(IComWrapper catalog, Entity entity, PropertyPurpose purpose)
        {
            using (IComWrapper attributes = catalog.GetAndWrap("Реквизиты"))
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
                    }
                }
            }
        }
        private void LoadStandardAttributes(IComWrapper catalog, Entity entity)
        {
            using (IComWrapper attributes = catalog.GetAndWrap("СтандартныеРеквизиты"))
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
                        //entity.Properties.Add()
                    }
                }
                attributes.Release(ref iterator);
            }
        }
        private void LoadCatalog(ComConnector connector, IComWrapper catalog, IComWrapper names)
        {
            Entity entity = new Entity() { Name = (string)catalog.Get("Имя") };

            Catalogs.Add(entity.Name, entity);
            entity.Namespace = GetNamespace("Справочники");

            LoadStandardAttributes(catalog, entity);
            LoadAttributes(catalog, entity, PropertyPurpose.Property);

            Table table;
            int count = (int)names.Call("Количество");
            for (int i = 0; i < count; i++)
            {
                using (IComWrapper row = names.CallAndWrap("Получить", i))
                {
                    table = new Table()
                    {
                        Entity = entity,
                        Schema = "dbo",
                        Name = (string)row.Get("ИмяТаблицыХранения"),
                        Purpose = GetTablePurpose((string)row.Get("Назначение"))
                    };
                    if (table.Purpose == TablePurpose.Main)
                    {
                        entity.Code = GetTypeCode(table.Name);
                    }
                }
            }
        }
    }
}
