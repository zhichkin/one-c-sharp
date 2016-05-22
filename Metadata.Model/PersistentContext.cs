using System;
using Zhichkin.ORM;
using System.Configuration;
using System.Collections.Generic;

namespace Zhichkin.Metadata.Model
{
    public sealed class MetadataPersistentContext : IPersistentContext
    {
        private static readonly IPersistentContext singelton;

        private const string name = "Zhichkin.Metadata";
        private static readonly string connectionString = string.Empty;
        private static readonly BiDictionary<int, Type> typeCodes = new BiDictionary<int, Type>();
        private static readonly Dictionary<Type, IDataMapper> mappers = new Dictionary<Type, IDataMapper>();
        private static readonly IReferenceObjectFactory factory = new ReferenceObjectFactory(typeCodes);

        static MetadataPersistentContext()
        {
            connectionString = ConfigurationManager.ConnectionStrings[name].ConnectionString;
            InitializeTypeCodes();
            InitializeDataMappers();
            singelton = new MetadataPersistentContext();
        }

        public MetadataPersistentContext() { }

        public static IPersistentContext Current
        {
            get { return singelton; }
        }

        private static void InitializeTypeCodes()
        {
            typeCodes.Add(1, typeof(InfoBase));
            typeCodes.Add(2, typeof(Namespace));
            typeCodes.Add(3, typeof(Entity));
            typeCodes.Add(4, typeof(Property));
            typeCodes.Add(5, typeof(Table));
            typeCodes.Add(6, typeof(Field));
            typeCodes.Add(7, typeof(PropertyType));
        }

        private static void InitializeDataMappers()
        {
            mappers.Add(typeof(InfoBase), new InfoBase.DataMapper(connectionString, factory));
            mappers.Add(typeof(Namespace), new Namespace.DataMapper(connectionString, factory));
            mappers.Add(typeof(Entity), new Entity.DataMapper(connectionString, factory));
            mappers.Add(typeof(Property), new Property.DataMapper(connectionString, factory));
            mappers.Add(typeof(Table), new Table.DataMapper(connectionString, factory));
            mappers.Add(typeof(Field), new Field.DataMapper(connectionString, factory));
            mappers.Add(typeof(PropertyType), new PropertyType.DataMapper(connectionString, factory));
        }

        public string Name { get { return name; } }
        public string ConnectionString { get { return connectionString; } }
        public IDataMapper GetDataMapper(Type type) { return mappers[type]; }
        public BiDictionary<int, Type> TypeCodes { get { return typeCodes; } }
        public IReferenceObjectFactory ReferenceObjectFactory { get { return factory; } }
    }
}
