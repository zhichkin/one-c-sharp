using System;
using Zhichkin.ORM;
using System.Configuration;
using System.Collections.Generic;

namespace Zhichkin.DXM.Model
{
    public sealed class DXMContext : IPersistentContext
    {
        private static readonly IPersistentContext singelton;

        private const string name = "Zhichkin.DXM";
        private static readonly string connectionString = string.Empty;
        private static readonly BiDictionary<int, Type> typeCodes = new BiDictionary<int, Type>();
        private static readonly Dictionary<Type, IDataMapper> mappers = new Dictionary<Type, IDataMapper>();
        private static readonly IReferenceObjectFactory factory = new ReferenceObjectFactory(typeCodes);

        static DXMContext()
        {
            connectionString = ConfigurationManager.ConnectionStrings[name].ConnectionString;
            InitializeTypeCodes();
            InitializeDataMappers();
            singelton = new DXMContext();
        }
        public DXMContext() { }

        public static IPersistentContext Current
        {
            get { return singelton; }
        }

        private static void InitializeTypeCodes()
        {
            typeCodes.Add(1, typeof(Publication));
        }

        private static void InitializeDataMappers()
        {
            mappers.Add(typeof(Publication), new Publication.DataMapper(connectionString, factory));
        }

        public string Name { get { return name; } }
        public string ConnectionString { get { return connectionString; } }
        public IDataMapper GetDataMapper(Type type) { return mappers[type]; }
        public BiDictionary<int, Type> TypeCodes { get { return typeCodes; } }
        public IReferenceObjectFactory Factory { get { return factory; } }
    }
}
