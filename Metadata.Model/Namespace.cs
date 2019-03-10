using System;
using System.Collections.Generic;
using Zhichkin.Hermes.Infrastructure;
using Zhichkin.Metadata.Services;
using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public sealed partial class Namespace : EntityBase, INamespaceInfo
    {
        private static readonly IDataMapper _mapper = MetadataPersistentContext.Current.GetDataMapper(typeof(Namespace));

        private static readonly IMetadataService service = new MetadataService();

        public Namespace() : base(_mapper) { }
        public Namespace(Guid identity) : base(_mapper, identity) { }
        public Namespace(Guid identity, PersistentState state) : base(_mapper, identity, state) { }

        private EntityBase owner = null; // InfoBase & Namespace
        public EntityBase Owner { set { Set<EntityBase>(value, ref owner); } get { return Get<EntityBase>(ref owner); } }
        
        public InfoBase InfoBase
        {
            get
            {
                Namespace currentNamespace = this;
                while (currentNamespace.Owner.GetType() != typeof(InfoBase))
                {
                    currentNamespace = (Namespace)currentNamespace.Owner;
                }
                return (InfoBase)currentNamespace.Owner;
            }
        }

        private List<Namespace> namespaces = new List<Namespace>();
        private List<Entity> entities = new List<Entity>();
        public IList<Namespace> Namespaces
        {
            get
            {
                if (this.state == PersistentState.New) return namespaces;
                if (namespaces.Count > 0) return namespaces;
                return service.GetChildren<Namespace, Namespace>(this, "owner");
            }
        }
        public IList<Entity> Entities
        {
            get
            {
                if (this.state == PersistentState.New) return entities;
                if (entities.Count > 0) return entities;
                return service.GetChildren<Namespace, Entity>(this, "namespace");
            }
        }

        IInfoBaseInfo INamespaceInfo.InfoBase { get { return this.InfoBase; } }
        INamespaceInfo INamespaceInfo.Namespace
        {
            get
            {
                return (this.Owner is Namespace) ? (INamespaceInfo)this.Owner : null;
            }
        }
    }
}