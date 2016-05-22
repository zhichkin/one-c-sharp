using System;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public sealed partial class Namespace : EntityBase
    {
        private static readonly IDataMapper _mapper = MetadataPersistentContext.Current.GetDataMapper(typeof(Namespace));

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

        public List<Entity> Entities { set; get; }
    }
}