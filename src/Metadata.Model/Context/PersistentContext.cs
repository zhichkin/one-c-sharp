using System;
using Zhichkin.ORM;
using System.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;
using System.IO;

namespace Zhichkin.Metadata.Model
{
    public sealed class MetadataPersistentContext : IPersistentContext
    {
        private static readonly IPersistentContext singelton;

        private const string SQL_FILES_FILTER = "*.sql";

        private const string name = "Zhichkin.Metadata";
        private static string connectionString = string.Empty;
        private static readonly BiDictionary<int, Type> typeCodes = new BiDictionary<int, Type>();
        private static readonly Dictionary<Type, IDataMapper> mappers = new Dictionary<Type, IDataMapper>();
        private static readonly IReferenceObjectFactory factory = new ReferenceObjectFactory(typeCodes);

        static MetadataPersistentContext()
        {
            singelton = new MetadataPersistentContext();

            var setting = ConfigurationManager.ConnectionStrings[name];
            connectionString = (setting == null) ? string.Empty : setting.ConnectionString;
            InitializeTypeCodes();
            InitializeDataMappers();
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
            typeCodes.Add(7, typeof(Relation));
            typeCodes.Add(8, typeof(CustomSetting));
            typeCodes.Add(9, typeof(Request));
        }

        private static void InitializeDataMappers()
        {
            mappers.Add(typeof(InfoBase), new InfoBase.DataMapper(connectionString, factory));
            mappers.Add(typeof(Namespace), new Namespace.DataMapper(connectionString, factory));
            mappers.Add(typeof(Entity), new Entity.DataMapper(connectionString, factory));
            mappers.Add(typeof(Property), new Property.DataMapper(connectionString, factory));
            mappers.Add(typeof(Table), new Table.DataMapper(connectionString, factory));
            mappers.Add(typeof(Field), new Field.DataMapper(connectionString, factory));
            mappers.Add(typeof(Relation), new Relation.DataMapper(connectionString, factory));
            mappers.Add(typeof(CustomSetting), new CustomSetting.DataMapper(connectionString, factory));
            mappers.Add(typeof(Request), new Request.DataMapper(connectionString, factory));
        }

        public string Name { get { return name; } }
        public string ConnectionString { get { return connectionString; } }
        public IDataMapper GetDataMapper(Type type) { return mappers[type]; }
        public BiDictionary<int, Type> TypeCodes { get { return typeCodes; } }
        public IReferenceObjectFactory Factory { get { return factory; } }

        public bool CheckDatabaseConnection()
        {
            string connectionString = Current.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return false;
            }

            bool result = false;
            {
                SqlConnection connection = new SqlConnection(connectionString);
                try
                {
                    connection.Open();
                    result = (connection.State == ConnectionState.Open);
                }
                catch
                {
                    // TODO: handle or log the error
                }
                finally
                {
                    if (connection != null) connection.Dispose();
                }
            }
            return result;
        }
        public void RefreshConnectionString()
        {
            connectionString = ConfigurationManager.ConnectionStrings[name].ConnectionString;
            mappers.Clear();
            InitializeDataMappers();
        }
        public void SetupDatabase()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string directory = Path.GetDirectoryName(asm.Location);
            string path = Path.Combine(directory, "SQL");
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Directory not found!{Environment.NewLine}{path}");
            }
            string[] script_files = Directory.GetFiles(path, SQL_FILES_FILTER);
            if (script_files == null || script_files.Length == 0)
            {
                throw new FileNotFoundException("SQL files not found!");
            }
            ExecuteDatabaseSetupScripts(script_files);
        }
        private void ExecuteDatabaseSetupScripts(string[] script_files)
        {
            {
                SqlConnection connection = new SqlConnection(ConnectionString);
                SqlCommand command = connection.CreateCommand();
                try
                {
                    connection.Open();

                    foreach (string script_path in script_files)
                    {
                        string sql = File.ReadAllText(script_path);
                        command.CommandType = CommandType.Text;
                        command.CommandText = sql;
                        _ = command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    if (command != null) { command.Dispose(); }
                    if (connection != null) { connection.Dispose(); }
                }
            }
        }
    }
}
