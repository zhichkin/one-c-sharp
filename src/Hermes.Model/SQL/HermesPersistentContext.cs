//using System;
//using System.Collections.Generic;
//using Zhichkin.Metadata.Model;
//using Zhichkin.ORM;

//namespace Zhichkin.Hermes.Model
//{
//    public sealed class HermesPersistentContext : IPersistentContext
//    {
//        private static readonly IPersistentContext singelton;

//        private const string name = "Zhichkin.Hermes";
//        private static readonly string connectionString = string.Empty;
//        private static readonly BiDictionary<int, Type> typeCodes = new BiDictionary<int, Type>();
//        private static readonly Dictionary<Type, IDataMapper> mappers = new Dictionary<Type, IDataMapper>();
//        private static readonly IReferenceObjectFactory factory = new ReferenceObjectFactory(typeCodes);

//        static HermesPersistentContext()
//        {
//            connectionString = MetadataPersistentContext.Current.ConnectionString;
//            InitializeTypeCodes();
//            InitializeDataMappers();
//            singelton = new HermesPersistentContext();
//        }
//        private HermesPersistentContext() { }
//        public static IPersistentContext Current
//        {
//            get { return singelton; }
//        }

//        public string Name { get { return name; } }
//        public string ConnectionString { get { return connectionString; } }
//        public IDataMapper GetDataMapper(Type type) { return mappers[type]; }
//        public BiDictionary<int, Type> TypeCodes { get { return typeCodes; } }
//        public IReferenceObjectFactory Factory { get { return factory; } }

//        private static void InitializeTypeCodes()
//        {
//            typeCodes.Add(1, typeof(Request));
//        }
//        private static void InitializeDataMappers()
//        {
//            mappers.Add(typeof(Request), new Request.DataMapper(connectionString, factory));
//        }
//    }
//}
