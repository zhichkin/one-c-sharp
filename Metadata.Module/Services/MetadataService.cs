using System;
using Zhichkin.ORM;
using Zhichkin.Metadata.Model;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Transactions;

namespace Zhichkin.Metadata.Services
{
    public sealed class MetadataService : IMetadataService
    {
        public InfoBase GetMetadata(string connectionString)
        {
            InfoBase infoBase = new InfoBase();
            IMetadataAdapter adapter = new XMLMetadataAdapter();
            adapter.Load(connectionString, infoBase);
            return infoBase;
        }

        public List<InfoBase> GetInfoBases()
        {
            throw new NotImplementedException();
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
        private void Save(Entity entity)
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
        private void Save(Property property)
        {
            if (property == null) throw new ArgumentNullException("property");

            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                property.Save();
                foreach (Entity type in property.Types)
                {
                    Relation relation = new Relation()
                    {
                        Type = type,
                        Property = property
                    };
                    Save(relation);
                }
                scope.Complete();
            }
        }
        private void Save(Relation relation)
        {
            if (relation == null) throw new ArgumentNullException("relation");

            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                relation.Save();
                scope.Complete();
            }
        }
        private void Save(Table table)
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
        private void Save(Field field)
        {
            if (field == null) throw new ArgumentNullException("field");

            TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                field.Save();
                scope.Complete();
            }
        }
    }
}