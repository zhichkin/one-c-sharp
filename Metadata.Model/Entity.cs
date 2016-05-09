using System;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public sealed partial class Entity : EntityBase
    {
        private static readonly IDataMapper _mapper = new Entity.DataMapper(
            PersistentContext.ConnectionString,
            new ReferenceObjectFactory(PersistentContext.TypeCodes));

        public Entity() : base(_mapper) { }
        public Entity(Guid identity) : base(_mapper, identity) { }
        public Entity(Guid identity, PersistentState state) : base(_mapper, identity, state) { }

        private int code = 0; // type code
        private Namespace _namespace = null; // Namespace
        private Entity owner = null; // Nesting, aggregation
        private Entity parent = null; // Inheritance

        public int Code { set { Set<int>(value, ref code); } get { return Get<int>(ref code); } }
        public Namespace Namespace { set { Set<Namespace>(value, ref _namespace); } get { return Get<Namespace>(ref _namespace); } }
        public Entity Owner { set { Set<Entity>(value, ref owner); } get { return Get<Entity>(ref owner); } }
        public Entity Parent { set { Set<Entity>(value, ref parent); } get { return Get<Entity>(ref parent); } }
        
        public string FullName { get { return string.Format("{0}.{1}", this.Namespace.Name, this.Name); } }
    }
}