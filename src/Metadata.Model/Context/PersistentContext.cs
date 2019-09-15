using System;
using Zhichkin.ORM;
using System.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;
using System.IO;
using Zhichkin.Metadata.Services;

namespace Zhichkin.Metadata.Model
{
    public sealed class MetadataPersistentContext : IPersistentContext
    {
        private static readonly IPersistentContext singelton;

        private const string SQL_FILES_FILTER = "*.sql";
        private const string SQL_SCHEMA_NAME = "metadata";

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

        public bool CheckServerConnection()
        {
            string connectionString = Current.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return false;
            }

            SqlConnectionStringBuilder helper = new SqlConnectionStringBuilder(connectionString);
            if (!string.IsNullOrWhiteSpace(helper.InitialCatalog))
            {
                helper.InitialCatalog = string.Empty;
            }

            bool result = false;
            {
                SqlConnection connection = new SqlConnection(helper.ToString());
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
        public bool CheckDatabaseConnection()
        {
            string connectionString = Current.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return false;
            }

            SqlConnectionStringBuilder helper = new SqlConnectionStringBuilder(connectionString);
            if (string.IsNullOrWhiteSpace(helper.InitialCatalog))
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
        private List<string> GetMetadataTableNames()
        {
            return new List<string>()
            {
                "infobases", "namespaces", "entities", "properties", "relations", "tables", "fields", "requests"
            };
        }
        private string SQLScript_CheckTableExists(string tableName)
        {
            return $"SELECT 1 FROM sys.tables AS t INNER JOIN sys.schemas AS s ON t.schema_id = s.schema_id AND s.name = '{SQL_SCHEMA_NAME}' AND t.name = '{tableName}'";
        }
        public bool CheckTables()
        {
            bool result = false;

            {
                SqlConnection connection = new SqlConnection(Current.ConnectionString);
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                try
                {
                    connection.Open();

                    int exists = 0;
                    object value = null;
                    foreach (string tableName in GetMetadataTableNames())
                    {
                        command.CommandText = SQLScript_CheckTableExists(tableName);
                        value = command.ExecuteScalar();
                        exists = (value == null) ? 0 : (int)value;
                        if (exists == 0) break;
                    }
                    result = (exists == 1);
                }
                catch
                {
                    // TODO: handle or log the error
                }
                finally
                {
                    if (command != null) command.Dispose();
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

        public void CreateMetaModel()
        {
            IMetadataService service = new MetadataService();
            InfoBase metadata = service.GetSystemInfoBase();

            Namespace root = new Namespace()
            {
                Owner = metadata,
                Name = "MetaModel"
            };
            root.Save();

            Entity infoBase = CreateInfoBase(root);
            Entity nameSpace = CreateNamespace(root, infoBase);
            Entity entity = CreateEntity(root, nameSpace);
            Entity property = CreateProperty(root, entity);
            Entity table = CreateTable(root, entity);
            CreateField(root, table, property);
            CreateRelation(root, property, entity);
            CreateRequest(root, nameSpace, entity);
        }
        
        private void AddIdentityProperty(Entity entity, Table table)
        {
            Property property = new Property()
            {
                Entity = entity,
                Ordinal = 0,
                Name = "Identity",
                IsReadOnly = true,
                IsPrimaryKey = true,
                Purpose = PropertyPurpose.System,
                IsAbstract = false
            };
            property.Save();

            Relation relation = new Relation()
            {
                Property = property,
                Entity = Entity.GUID
            };
            relation.Save();

            Field field = new Field()
            {
                Property = property,
                Table = table,
                Name = "key",
                TypeName = "uniqueidentifier",
                IsNullable = false,
                IsPrimaryKey = true,
                KeyOrdinal = 1,
                Length = 16,
                Scale = 0,
                Precision = 0,
                Purpose = FieldPurpose.Value
            };
            field.Save();
        }
        private void AddVersionProperty(Entity entity, Table table)
        {
            Property property = new Property()
            {
                Entity = entity,
                Ordinal = 1,
                Name = "Version",
                IsReadOnly = false,
                IsPrimaryKey = false,
                Purpose = PropertyPurpose.System,
                IsAbstract = false
            };
            property.Save();

            Relation relation = new Relation()
            {
                Property = property,
                Entity = Entity.Binary
            };
            relation.Save();

            Field field = new Field()
            {
                Property = property,
                Table = table,
                Name = "version",
                TypeName = "rowversion",
                IsNullable = false,
                IsPrimaryKey = false,
                KeyOrdinal = 0,
                Length = 8,
                Scale = 0,
                Precision = 0,
                Purpose = FieldPurpose.Value
            };
            field.Save();
        }
        private void AddStringProperty(Entity entity, Table table, int ordinal, string propertyName, string fieldName, int size)
        {
            Property property = new Property()
            {
                Entity = entity,
                Ordinal = ordinal,
                Name = propertyName,
                IsReadOnly = false,
                IsPrimaryKey = false,
                IsAbstract = false,
                Purpose = PropertyPurpose.Property
            };
            property.Save();

            Relation relation = new Relation()
            {
                Property = property,
                Entity = Entity.String
            };
            relation.Save();

            Field field = new Field()
            {
                Property = property,
                Table = table,
                Name = fieldName,
                TypeName = "nvarchar",
                IsNullable = false,
                IsPrimaryKey = false,
                KeyOrdinal = 0,
                Length = size,
                Scale = 0,
                Precision = 0,
                Purpose = FieldPurpose.Value
            };
            field.Save();
        }
        private void AddObjectProperty(Entity entity, Table table, int ordinal, string propertyName, Entity type, string fieldName)
        {
            Property property = new Property()
            {
                Entity = entity,
                Ordinal = ordinal,
                Name = propertyName,
                IsReadOnly = false,
                IsPrimaryKey = false,
                IsAbstract = false,
                Purpose = PropertyPurpose.Property
            };
            property.Save();

            Relation relation = new Relation()
            {
                Property = property,
                Entity = type
            };
            relation.Save();

            Field field = new Field()
            {
                Property = property,
                Table = table,
                Name = fieldName,
                TypeName = "uniqueidentifier",
                IsNullable = false,
                IsPrimaryKey = false,
                KeyOrdinal = 0,
                Length = 16,
                Scale = 0,
                Precision = 0,
                Purpose = FieldPurpose.Object
            };
            field.Save();
        }
        private void AddIntegerProperty(Entity entity, Table table, int ordinal, string propertyName, string fieldName)
        {
            Property property = new Property()
            {
                Entity = entity,
                Ordinal = ordinal,
                Name = propertyName,
                IsReadOnly = false,
                IsPrimaryKey = false,
                IsAbstract = false,
                Purpose = PropertyPurpose.Property
            };
            property.Save();

            Relation relation = new Relation()
            {
                Property = property,
                Entity = Entity.Int32
            };
            relation.Save();

            Field field = new Field()
            {
                Property = property,
                Table = table,
                Name = fieldName,
                TypeName = "int",
                IsNullable = false,
                IsPrimaryKey = false,
                KeyOrdinal = 0,
                Length = 4,
                Scale = 0,
                Precision = 10,
                Purpose = FieldPurpose.Value
            };
            field.Save();
        }
        private void AddBooleanProperty(Entity entity, Table table, int ordinal, string propertyName, string fieldName)
        {
            Property property = new Property()
            {
                Entity = entity,
                Ordinal = ordinal,
                Name = propertyName,
                IsReadOnly = false,
                IsPrimaryKey = false,
                IsAbstract = false,
                Purpose = PropertyPurpose.Property
            };
            property.Save();

            Relation relation = new Relation()
            {
                Property = property,
                Entity = Entity.Boolean
            };
            relation.Save();

            Field field = new Field()
            {
                Property = property,
                Table = table,
                Name = fieldName,
                TypeName = "bit",
                IsNullable = false,
                IsPrimaryKey = false,
                KeyOrdinal = 0,
                Length = 1,
                Scale = 0,
                Precision = 1,
                Purpose = FieldPurpose.Value
            };
            field.Save();
        }

        private Entity CreateInfoBase(Namespace root)
        {
            Entity entity = new Entity()
            {
                Namespace = root,
                Name = "InfoBase",
                Alias = "Информационная база",
                Code = 1,
                IsSealed = false,
                IsAbstract = false
            };
            entity.Save();

            Table table = new Table()
            {
                Entity = entity,
                Schema = "metadata",
                Name = "infobases",
                Purpose = TablePurpose.Main
            };
            table.Save();

            AddIdentityProperty(entity, table);
            AddVersionProperty(entity, table);
            AddStringProperty(entity, table, 2, "Name", "name", 100);
            AddStringProperty(entity, table, 3, "Server", "server", 100);
            AddStringProperty(entity, table, 4, "Database", "database", 100);
            AddStringProperty(entity, table, 5, "UserName", "username", 100);
            AddStringProperty(entity, table, 6, "Password", "password", 100);

            return entity;
        }
        private Entity CreateNamespace(Namespace root, Entity infoBase)
        {
            Entity entity = new Entity()
            {
                Namespace = root,
                Name = "Namespace",
                Alias = "Пространство имён",
                Code = 2,
                IsSealed = false,
                IsAbstract = false
            };
            entity.Save();

            Table table = new Table()
            {
                Entity = entity,
                Schema = "metadata",
                Name = "namespaces",
                Purpose = TablePurpose.Main
            };
            table.Save();

            AddIdentityProperty(entity, table);
            AddVersionProperty(entity, table);
            AddStringProperty(entity, table, 2, "Name", "name", 100);

            Property property = new Property()
            {
                Entity = entity,
                Ordinal = 3,
                Name = "Owner",
                IsReadOnly = false,
                IsPrimaryKey = false,
                IsAbstract = false,
                Purpose = PropertyPurpose.Property
            };
            property.Save();

            Relation relation = new Relation()
            {
                Property = property,
                Entity = infoBase
            };
            relation.Save();

            relation = new Relation()
            {
                Property = property,
                Entity = entity
            };
            relation.Save();

            Field field = new Field()
            {
                Property = property,
                Table = table,
                Name = "owner",
                TypeName = "uniqueidentifier",
                IsNullable = false,
                IsPrimaryKey = false,
                KeyOrdinal = 0,
                Length = 16,
                Scale = 0,
                Precision = 0,
                Purpose = FieldPurpose.Object
            };
            field.Save();

            field = new Field()
            {
                Property = property,
                Table = table,
                Name = "owner_",
                TypeName = "int",
                IsNullable = false,
                IsPrimaryKey = false,
                KeyOrdinal = 0,
                Length = 4,
                Scale = 0,
                Precision = 10,
                Purpose = FieldPurpose.TypeCode
            };
            field.Save();

            return entity;
        }
        private Entity CreateEntity(Namespace root, Entity nameSpace)
        {
            Entity entity = new Entity()
            {
                Namespace = root,
                Name = "Entity",
                Alias = "Объект метаданных",
                Code = 3,
                IsSealed = false,
                IsAbstract = false
            };
            entity.Save();

            Table table = new Table()
            {
                Entity = entity,
                Schema = "metadata",
                Name = "entities",
                Purpose = TablePurpose.Main
            };
            table.Save();

            AddIdentityProperty(entity, table);
            AddVersionProperty(entity, table);
            AddStringProperty(entity, table, 2, "Name", "name", 100);
            AddStringProperty(entity, table, 3, "Alias", "alias", 128);
            AddObjectProperty(entity, table, 4, "Namespace", nameSpace, "namespace");
            AddObjectProperty(entity, table, 5, "Owner", entity, "owner");
            AddObjectProperty(entity, table, 6, "Parent", entity, "parent");
            AddBooleanProperty(entity, table, 7, "IsAbstract", "is_abstract");
            AddBooleanProperty(entity, table, 8, "IsSealed", "is_sealed");

            return entity;
        }
        private Entity CreateProperty(Namespace root, Entity entityType)
        {
            Entity entity = new Entity()
            {
                Namespace = root,
                Name = "Property",
                Alias = "Свойство объекта метаданных",
                Code = 4,
                IsSealed = false,
                IsAbstract = false
            };
            entity.Save();

            Table table = new Table()
            {
                Entity = entity,
                Schema = "metadata",
                Name = "properties",
                Purpose = TablePurpose.Main
            };
            table.Save();

            AddIdentityProperty(entity, table);
            AddVersionProperty(entity, table);
            AddStringProperty(entity, table, 2, "Name", "name", 128);
            AddObjectProperty(entity, table, 3, "Entity", entityType, "entity");
            AddIntegerProperty(entity, table, 4, "Purpose", "purpose");
            AddIntegerProperty(entity, table, 5, "Ordinal", "ordinal");
            AddBooleanProperty(entity, table, 6, "IsAbstract", "is_abstract");
            AddBooleanProperty(entity, table, 7, "IsReadOnly", "is_read_only");
            AddBooleanProperty(entity, table, 8, "IsPrimaryKey", "is_primary_key");

            return entity;
        }
        private Entity CreateTable(Namespace root, Entity entityType)
        {
            Entity entity = new Entity()
            {
                Namespace = root,
                Name = "Table",
                Alias = "Таблица СУБД объекта метаданных",
                Code = 5,
                IsSealed = false,
                IsAbstract = false
            };
            entity.Save();

            Table table = new Table()
            {
                Entity = entity,
                Schema = "metadata",
                Name = "tables",
                Purpose = TablePurpose.Main
            };
            table.Save();

            AddIdentityProperty(entity, table);
            AddVersionProperty(entity, table);
            AddStringProperty(entity, table, 2, "Name", "name", 100);
            AddStringProperty(entity, table, 3, "Schema", "schema", 100);
            AddObjectProperty(entity, table, 4, "Entity", entityType, "entity");
            AddIntegerProperty(entity, table, 5, "Purpose", "purpose");

            return entity;
        }
        private void CreateField(Namespace root, Entity tableType, Entity propertyType)
        {
            Entity entity = new Entity()
            {
                Namespace = root,
                Name = "Field",
                Alias = "Поле таблицы СУБД объекта метаданных",
                Code = 6,
                IsSealed = false,
                IsAbstract = false
            };
            entity.Save();

            Table table = new Table()
            {
                Entity = entity,
                Schema = "metadata",
                Name = "fields",
                Purpose = TablePurpose.Main
            };
            table.Save();

            AddIdentityProperty(entity, table);
            AddVersionProperty(entity, table);
            AddStringProperty(entity, table, 2, "Name", "name", 100);
            AddObjectProperty(entity, table, 3, "Table", tableType, "table");
            AddObjectProperty(entity, table, 4, "Property", propertyType, "property");
            AddIntegerProperty(entity, table, 5, "Purpose", "purpose");
            AddStringProperty(entity, table, 6, "TypeName", "type_name", 16);
            AddIntegerProperty(entity, table, 7, "Length", "length");
            AddIntegerProperty(entity, table, 8, "Precision", "precision");
            AddIntegerProperty(entity, table, 9, "Scale", "scale");
            AddBooleanProperty(entity, table, 10, "IsNullable", "is_nullable");
            AddBooleanProperty(entity, table, 11, "IsPrimaryKey", "is_primary_key");

            Property property = new Property()
            {
                Entity = entity,
                Ordinal = 12,
                Name = "KeyOrdinal",
                IsReadOnly = false,
                IsPrimaryKey = false,
                IsAbstract = false,
                Purpose = PropertyPurpose.Property
            };
            property.Save();
            Relation relation = new Relation()
            {
                Property = property,
                Entity = Entity.Byte
            };
            relation.Save();
            Field field = new Field()
            {
                Property = property,
                Table = table,
                Name = "key_ordinal",
                TypeName = "tinyint",
                IsNullable = false,
                IsPrimaryKey = false,
                KeyOrdinal = 0,
                Length = 1,
                Scale = 0,
                Precision = 3,
                Purpose = FieldPurpose.Value
            };
            field.Save();
        }
        private void CreateRelation(Namespace root, Entity propertyType, Entity entityType)
        {
            Entity entity = new Entity()
            {
                Namespace = root,
                Name = "Relation",
                Alias = "Допустимые для свойства объекта типы данных",
                Code = 7,
                IsSealed = false,
                IsAbstract = false
            };
            entity.Save();

            Table table = new Table()
            {
                Entity = entity,
                Schema = "metadata",
                Name = "relations",
                Purpose = TablePurpose.Main
            };
            table.Save();

            Property property = new Property()
            {
                Entity = entity,
                Ordinal = 1,
                Name = "Property",
                IsReadOnly = false,
                IsPrimaryKey = true,
                IsAbstract = false,
                Purpose = PropertyPurpose.Property
            };
            property.Save();
            Relation relation = new Relation()
            {
                Property = property,
                Entity = propertyType
            };
            relation.Save();
            Field field = new Field()
            {
                Property = property,
                Table = table,
                Name = "property",
                TypeName = "uniqueidentifier",
                IsNullable = false,
                IsPrimaryKey = true,
                KeyOrdinal = 1,
                Length = 16,
                Scale = 0,
                Precision = 0,
                Purpose = FieldPurpose.Object
            };
            field.Save();

            property = new Property()
            {
                Entity = entity,
                Ordinal = 2,
                Name = "Entity",
                IsReadOnly = false,
                IsPrimaryKey = true,
                IsAbstract = false,
                Purpose = PropertyPurpose.Property
            };
            property.Save();
            relation = new Relation()
            {
                Property = property,
                Entity = entityType
            };
            relation.Save();
            field = new Field()
            {
                Property = property,
                Table = table,
                Name = "entity",
                TypeName = "uniqueidentifier",
                IsNullable = false,
                IsPrimaryKey = true,
                KeyOrdinal = 2,
                Length = 16,
                Scale = 0,
                Precision = 0,
                Purpose = FieldPurpose.Object
            };
            field.Save();
        }
        private void CreateRequest(Namespace root, Entity nameSpace, Entity entityType)
        {
            Entity entity = new Entity()
            {
                Namespace = root,
                Name = "Request",
                Alias = "Запрос данных",
                Code = 9,
                IsSealed = false,
                IsAbstract = false
            };
            entity.Save();

            Table table = new Table()
            {
                Entity = entity,
                Schema = "metadata",
                Name = "requests",
                Purpose = TablePurpose.Main
            };
            table.Save();

            AddIdentityProperty(entity, table);
            AddVersionProperty(entity, table);
            AddStringProperty(entity, table, 2, "Name", "name", 100);
            AddObjectProperty(entity, table, 3, "Namespace", nameSpace, "namespace");
            AddObjectProperty(entity, table, 4, "Owner", entityType, "owner");
            AddStringProperty(entity, table, 5, "ParseTree", "parse_tree", -1);
            AddObjectProperty(entity, table, 6, "RequestType", entityType, "request_type");
            AddObjectProperty(entity, table, 7, "ResponseType", entityType, "response_type");
        }
    }
}
