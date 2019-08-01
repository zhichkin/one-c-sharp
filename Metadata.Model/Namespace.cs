using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Zhichkin.Metadata.Services;
using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public sealed partial class Namespace : EntityBase
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

        //private List<Namespace> namespaces = new List<Namespace>();
        //private List<Entity> entities = new List<Entity>();
        public IList<Namespace> Namespaces
        {
            get
            {
                //if (this.state == PersistentState.New) return namespaces;
                //if (namespaces.Count > 0) return namespaces;
                return service.GetChildren<Namespace, Namespace>(this, "owner");
            }
        }
        public IList<Entity> Entities
        {
            get
            {
                //if (this.state == PersistentState.New) return entities;
                //if (entities.Count > 0) return entities;
                return service.GetChildren<Namespace, Entity>(this, "namespace");
            }
        }

        private ObservableCollection<Entity> _ObservableEntities;
        public ObservableCollection<Entity> ObservableEntities
        {
            set { _ObservableEntities = value; }
            get
            {
                if (_ObservableEntities == null)
                {
                    _ObservableEntities = new ObservableCollection<Entity>(this.Entities);
                }
                return _ObservableEntities;
            }
        }

        private ObservableCollection<Namespace> _ObservableNamespaces;
        public ObservableCollection<Namespace> ObservableNamespaces
        {
            set { _ObservableNamespaces = value; }
            get
            {
                if (_ObservableNamespaces == null)
                {
                    _ObservableNamespaces = new ObservableCollection<Namespace>(this.Namespaces);
                }
                return _ObservableNamespaces;
            }
        }

        public IList<Request> Requests
        {
            get
            {
                return service.GetChildren<Namespace, Request>(this, "namespace");
            }
        }
        private ObservableCollection<Request> _ObservableRequests;
        public ObservableCollection<Request> ObservableRequests
        {
            set { _ObservableRequests = value; }
            get
            {
                if (_ObservableRequests == null)
                {
                    _ObservableRequests = new ObservableCollection<Request>(this.Requests);
                }
                return _ObservableRequests;
            }
        }
    }
}