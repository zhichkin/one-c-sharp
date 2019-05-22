using System;
using System.Collections.Generic;
using Zhichkin.Metadata.Model;
using Zhichkin.ORM;
using System.Data.SqlClient;

namespace Zhichkin.Metadata.Services
{
    public interface IMetadataService
    {
        InfoBase GetSystemInfoBase();
        Namespace GetTypeSystemNamespace();

        List<InfoBase> GetInfoBases();

        IList<TChild> GetChildren<TParent, TChild>(TParent entity, string propertyName)
            where TParent : IReferenceObject
            where TChild : IReferenceObject;

        IList<TChild> GetChildren<TParent, TChild>(TParent entity, string propertyName, Action<SqlDataReader, TChild> mapper)
            where TParent : IReferenceObject
            where TChild : IValueObject, new();

        void Save(InfoBase entity);
        void Save(Namespace entity);
        void Save(Entity entity);
        void Save(Property entity);
        void Save(Relation entity);
        void Save(Table entity);
        void Save(Field entity);
        void Kill(InfoBase entity);
        void Kill(Namespace entity);
        void Kill(Entity entity);
        void Kill(Property entity);
        void Kill(Relation entity);
        void Kill(Table entity);
        void Kill(Field entity);
    }
}
