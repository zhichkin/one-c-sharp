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
            typeCodes.Add(2, typeof(Article));
            typeCodes.Add(3, typeof(PublicationSubscriber));
            typeCodes.Add(4, typeof(Subscription));
            typeCodes.Add(5, typeof(Mapping));
            typeCodes.Add(6, typeof(PublicationProperty));
        }

        private static void InitializeDataMappers()
        {
            mappers.Add(typeof(Publication), new Publication.DataMapper(connectionString, factory));
            mappers.Add(typeof(Article), new Article.DataMapper(connectionString, factory));
            mappers.Add(typeof(PublicationSubscriber), new PublicationSubscriber.DataMapper(connectionString, factory));
            mappers.Add(typeof(Subscription), new Subscription.DataMapper(connectionString, factory));
            mappers.Add(typeof(Mapping), new Mapping.DataMapper(connectionString, factory));
            mappers.Add(typeof(PublicationProperty), new PublicationProperty.DataMapper(connectionString, factory));
        }

        public string Name { get { return name; } }
        public string ConnectionString { get { return connectionString; } }
        public IDataMapper GetDataMapper(Type type) { return mappers[type]; }
        public BiDictionary<int, Type> TypeCodes { get { return typeCodes; } }
        public IReferenceObjectFactory Factory { get { return factory; } }
    }
}
