using System;
using Zhichkin.ORM;
using Zhichkin.Metadata.Model;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Transactions;
using System.Text;

namespace Zhichkin.Metadata.Services
{
    public sealed class MetadataService : IMetadataService
    {
        public MetadataService()
        {
            Factory          = MetadataPersistentContext.Current.Factory;
            ConnectionString = MetadataPersistentContext.Current.ConnectionString;
        }
        public IReferenceObjectFactory Factory { get; private set; }
        public string ConnectionString { get; private set; }

        public List<InfoBase> GetInfoBases()
        {
            List<InfoBase> list = new List<InfoBase>();

            QueryService service = new QueryService(MetadataPersistentContext.Current.ConnectionString);
            string sql = "SELECT [key] FROM [metadata].[infobases] WHERE [key] <> CAST(0x00000000000000000000000000000000 AS uniqueidentifier);";
            foreach (dynamic item in service.Execute(sql))
            {
                list.Add(new InfoBase((Guid)item.key, PersistentState.Virtual));
            }
            return list;
        }
                       
        public void Save(InfoBase infoBase)
        {
            if (infoBase == null) throw new ArgumentNullException("infoBase");

            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                infoBase.Save();
                foreach (Namespace nspace in infoBase.Namespaces)
                {
                    Save(nspace);
                }
                scope.Complete();
            }
        }
        public void Save(Namespace nspace)
        {
            if (nspace == null) throw new ArgumentNullException("nspace");

            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                nspace.Save();
                foreach (Entity entity in nspace.Entities)
                {
                    Save(entity);                    
                }
                foreach (Namespace child in nspace.Namespaces)
                {
                    Save(child);
                }
                scope.Complete();
            }
        }
        public void Save(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                entity.Save();
                foreach (Property property in entity.Properties)
                {
                    Save(property);
                }
                foreach (Entity child in entity.NestedEntities)
                {
                    Save(child);
                }
                foreach (Table table in entity.Tables)
                {
                    Save(table);
                }
                scope.Complete();
            }
        }
        public void Save(Property property)
        {
            if (property == null) throw new ArgumentNullException("property");

            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                property.Save();
                foreach (Relation relation in property.Relations)
                {
                    Save(relation);
                }
                scope.Complete();
            }
        }
        public void Save(Relation relation)
        {
            if (relation == null) throw new ArgumentNullException("relation");

            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                relation.Save();
                scope.Complete();
            }
        }
        public void Save(Table table)
        {
            if (table == null) throw new ArgumentNullException("table");

            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                table.Save();
                foreach (Field field in table.Fields)
                {
                    Save(field);
                }
                scope.Complete();
            }
        }
        public void Save(Field field)
        {
            if (field == null) throw new ArgumentNullException("field");

            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                field.Save();
                scope.Complete();
            }
        }

        public void Kill(InfoBase infoBase)
        {
            if (infoBase == null) throw new ArgumentNullException("infoBase");

            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                foreach (Namespace nspace in infoBase.Namespaces)
                {
                    Kill(nspace);
                }
                infoBase.Kill();
                scope.Complete();
            }
        }
        public void Kill(Namespace nspace)
        {
            if (nspace == null) throw new ArgumentNullException("nspace");

            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                foreach (Namespace child in nspace.Namespaces)
                {
                    Kill(child);
                }
                foreach (Entity entity in nspace.Entities)
                {
                    Kill(entity);
                }
                nspace.Kill();
                scope.Complete();
            }
        }
        public void Kill(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                foreach (Table table in entity.Tables)
                {
                    Kill(table);
                }
                foreach (Entity child in entity.NestedEntities)
                {
                    Kill(child);
                }
                foreach (Property property in entity.Properties)
                {
                    Kill(property);
                }
                entity.Kill();
                scope.Complete();
            }
        }
        public void Kill(Property property)
        {
            if (property == null) throw new ArgumentNullException("property");

            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                foreach (Relation relation in property.Relations)
                {
                    Kill(relation);
                }
                property.Kill();
                scope.Complete();
            }
        }
        public void Kill(Relation relation)
        {
            if (relation == null) throw new ArgumentNullException("relation");

            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                relation.Kill();
                scope.Complete();
            }
        }
        public void Kill(Table table)
        {
            if (table == null) throw new ArgumentNullException("table");

            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                foreach (Field field in table.Fields)
                {
                    Kill(field);
                }
                table.Kill();
                scope.Complete();
            }
        }
        public void Kill(Field field)
        {
            if (field == null) throw new ArgumentNullException("field");

            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                field.Kill();
                scope.Complete();
            }
        }

        public IList<TChild> GetChildren<TParent, TChild>(TParent entity, string propertyName)
            where TParent : IReferenceObject
            where TChild : IReferenceObject
        {
            List<TChild> list = new List<TChild>();

            Entity test = entity as Entity;
            if (test != null && test.Namespace == Namespace.TypeSystem) return list;

            string sql = @"SELECT [key] FROM [metadata].[{table_name}] WHERE [{fk_name}] = @key {filter} ORDER BY [name] ASC"
                .Replace("{table_name}", GetTableName<TChild>())
                .Replace("{fk_name}", propertyName)
                .Replace("{filter}", GetWhereClause<TParent, TChild>());

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = sql;

                SqlParameter parameter = new SqlParameter("key", System.Data.SqlDbType.UniqueIdentifier)
                {
                    Direction = System.Data.ParameterDirection.Input,
                    Value = entity.Identity
                };
                command.Parameters.Add(parameter);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(Factory.New<TChild>(reader.GetGuid(0)));
                }
            }

            return list;
        }
        public IList<TChild> GetChildren<TParent, TChild>(TParent entity, string propertyName, Action<SqlDataReader, TChild> mapper)
            where TParent : IReferenceObject
            where TChild : IValueObject, new()
        {
            List<TChild> list = new List<TChild>();

            string sql = @"SELECT {fields_list} FROM [metadata].[{table_name}] WHERE [{fk_name}] = @key"
                .Replace("{fields_list}", GetValueFields<TChild>())
                .Replace("{table_name}", GetTableName<TChild>())
                .Replace("{fk_name}", propertyName);

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = sql;

                SqlParameter parameter = new SqlParameter("key", System.Data.SqlDbType.UniqueIdentifier)
                {
                    Direction = System.Data.ParameterDirection.Input,
                    Value = entity.Identity
                };
                command.Parameters.Add(parameter);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    TChild child = new TChild();
                    child.State = PersistentState.Loading;
                    mapper(reader, child);
                    child.State = PersistentState.Original;
                    list.Add(child);
                }
            }

            return list;
        }
        private string GetTableName<T>() where T : IPersistent
        {
            string tableName = string.Empty;

            if (typeof(T) == typeof(InfoBase))
            {
                return "infobases";
            }
            else if (typeof(T) == typeof(Namespace))
            {
                return "namespaces";
            }
            else if (typeof(T) == typeof(Entity))
            {
                return "entities";
            }
            else if (typeof(T) == typeof(Property))
            {
                return "properties";
            }
            else if (typeof(T) == typeof(Table))
            {
                return "tables";
            }
            else if (typeof(T) == typeof(Field))
            {
                return "fields";
            }
            else if (typeof(T) == typeof(Relation))
            {
                return "relations";
            }

            return tableName;
        }
        private string GetValueFields<T>()
        {
            string list = string.Empty;

            if (typeof(T) == typeof(Relation))
            {
                return "[entity], [property]";
            }

            return list;
        }
        private string GetWhereClause<TParent, TChild>()
        {
            if (typeof(TParent) == typeof(Namespace) && typeof(TChild) == typeof(Entity))
            {
                return "AND [owner] = CAST(0x00000000000000000000000000000000 AS uniqueidentifier)"; // filter by nested entities
            }
            return string.Empty;
        }

        public Entity GetEntityInfo(InfoBase infoBase, int code)
        {
            Entity info = null;

            IPersistentContext context = MetadataPersistentContext.Current;
            QueryService service = new QueryService(context.ConnectionString);
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("WITH namespaces([key], [owner]) AS");
            sql.AppendLine("(");
            sql.Append("SELECT [key], [owner] FROM [metadata].[namespaces] WHERE [owner] = '");
            sql.Append(infoBase.Identity.ToString());
            sql.AppendLine("'");
            sql.AppendLine("UNION ALL");
            sql.AppendLine("SELECT n.[key], n.[owner] FROM [metadata].[namespaces] AS n");
            sql.AppendLine("INNER JOIN namespaces AS anchor ON anchor.[key] = n.[owner]");
            sql.AppendLine(")");
            sql.AppendLine("SELECT e.[key], e.[name] FROM [metadata].[entities] AS e");
            sql.AppendLine("INNER JOIN namespaces AS n");
            sql.Append("ON e.[namespace] = n.[key] AND e.[code] = ");
            sql.Append(code.ToString());
            sql.Append(";");
            foreach (dynamic item in service.Execute(sql.ToString()))
            {
                info = context.Factory.New<Entity>((Guid)item.key);
            }
            return info;
        }
        public Entity GetEntityInfo(InfoBase infoBase, string namespaceName, string entityName)
        {
            Entity info = null;

            IPersistentContext context = MetadataPersistentContext.Current;
            QueryService service = new QueryService(context.ConnectionString);
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("WITH namespaces([key], [owner], [name]) AS");
            sql.AppendLine("(");
            sql.Append("SELECT [key], [owner], [name] FROM [metadata].[namespaces] WHERE [owner] = '");
            sql.Append(infoBase.Identity.ToString());
            sql.AppendLine("'");
            sql.AppendLine("UNION ALL");
            sql.AppendLine("SELECT n.[key], n.[owner], n.[name] FROM [metadata].[namespaces] AS n");
            sql.AppendLine("INNER JOIN namespaces AS anchor ON anchor.[key] = n.[owner]");
            sql.AppendLine(")");
            sql.AppendLine("SELECT e.[key], e.[name] FROM [metadata].[entities] AS e");
            sql.AppendLine("INNER JOIN namespaces AS n");
            sql.Append("ON e.[namespace] = n.[key] AND n.[name] = N'");
            sql.Append(namespaceName);
            sql.Append("' AND e.[name] = N'");
            sql.Append(entityName);
            sql.Append("' AND e.[owner] = '00000000-0000-0000-0000-000000000000';");
            foreach (dynamic item in service.Execute(sql.ToString()))
            {
                info = context.Factory.New<Entity>((Guid)item.key);
            }
            return info;
        }
    }
}