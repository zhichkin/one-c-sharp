using System;
using Zhichkin.ORM;
using System.Configuration;
using System.Collections.Generic;

namespace Zhichkin.Integrator.Model
{
    public sealed class IntegratorPersistentContext : IPersistentContext
    {
        private static readonly IPersistentContext singelton;

        private const string name = "Zhichkin.Integrator";
        private static readonly string connectionString = string.Empty;
        private static readonly BiDictionary<int, Type> typeCodes = new BiDictionary<int, Type>();
        private static readonly Dictionary<Type, IDataMapper> mappers = new Dictionary<Type, IDataMapper>();
        private static readonly IReferenceObjectFactory factory = new ReferenceObjectFactory(typeCodes);

        static IntegratorPersistentContext()
        {
            connectionString = ConfigurationManager.ConnectionStrings[name].ConnectionString;
            InitializeTypeCodes();
            InitializeDataMappers();
            singelton = new IntegratorPersistentContext();
        }
        private IntegratorPersistentContext() { }
        public static IPersistentContext Current
        {
            get { return singelton; }
        }

        public string Name { get { return name; } }
        public string ConnectionString { get { return connectionString; } }
        public IDataMapper GetDataMapper(Type type) { return mappers[type]; }
        public BiDictionary<int, Type> TypeCodes { get { return typeCodes; } }
        public IReferenceObjectFactory Factory { get { return factory; } }

        private static void InitializeTypeCodes()
        {
            typeCodes.Add(1, typeof(Publisher));
        }
        private static void InitializeDataMappers()
        {
            mappers.Add(typeof(Publisher), new Publisher.DataMapper(connectionString, factory));
        }
    }
}
