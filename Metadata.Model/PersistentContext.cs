using System;
using Zhichkin.ORM;
using System.Configuration;

namespace Zhichkin.Metadata.Model
{
    public static class PersistentContext
    {
        public const string ModuleName = "Zhichkin.Metadata";

        private static string connectionString = string.Empty;
        private static readonly BiDictionary<int, Type> typeCodes = new BiDictionary<int, Type>();

        static PersistentContext()
        {
            //ConfigurationSection section = (ConfigurationSection)ConfigurationManager.GetSection(ModuleSettings.ModuleName);
            connectionString = ConfigurationManager.ConnectionStrings[PersistentContext.ModuleName].ConnectionString;
            InitializeTypeCodes();
        }

        private static void InitializeTypeCodes()
        {
            typeCodes.Add(1, typeof(InfoBase));
            typeCodes.Add(2, typeof(Namespace));
        }

        public static string ConnectionString { get { return connectionString; } }
        public static BiDictionary<int, Type> TypeCodes { get { return typeCodes; } }
    }
}
