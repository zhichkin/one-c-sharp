using System;
using Zhichkin.Metadata.Model;
using System.Collections.Generic;

namespace Zhichkin.Metadata.Services
{
    public sealed class MetadataService : IMetadataService
    {
        # region " Get 1C metadata "

        public InfoBase GetMetadata(string connectionString)
        {
            using (ComConnector connector = new ComConnector(connectionString))
            {
                connector.Connect();

                List<Entity> catalogs = LoadCatalogs(connector);
            }
            return null;
        }

        private List<Entity> LoadCatalogs(ComConnector connector)
        {
            List<Entity> list = new List<Entity>();

            using(IComWrapper metadata = connector.Metadata)
            using (IComWrapper catalogs = metadata.GetAndWrap("Справочники"))
            {
                int count = (int)catalogs.Call("Количество");
                for (int i = 0; i < count; i++)
                {
                    using (IComWrapper catalog = catalogs.CallAndWrap("Получить", i))
                    {
                        using (IComWrapper table = connector.GetDBNames(catalog))
                        {
                            list.Add(LoadCatalog(catalog, table));
                        }
                    }
                }
            }

            return list;
        }
        private Entity LoadCatalog(IComWrapper catalog, IComWrapper table)
        {
            Entity entity = null;

            int count = (int)table.Call("Количество");
            for (int i = 0; i < count; i++)
            {
                using (IComWrapper row = table.CallAndWrap("Получить", i))
                {
                    string value = (string)row.Get("ИмяТаблицыХранения");
                }
            }

            return null;
        }

        # endregion


        public List<InfoBase> GetInfoBases()
        {
            throw new NotImplementedException();
        }
    }
}
