using System;
using Zhichkin.Metadata.Model;
using Zhichkin.ORM;

namespace Zhichkin.Hermes.Model
{
    public sealed partial class Request : EntityBase
    {
        private static readonly IDataMapper _mapper = HermesPersistentContext.Current.GetDataMapper(typeof(Request));

        private Namespace _namespace = null;
        private Entity owner = null;
        private string parseTree = string.Empty;
        private Entity requestType = null;
        private Entity responseType = null;

        public Request() : base(_mapper) { }
        public Request(Guid identity) : base(_mapper, identity) { }
        public Request(Guid identity, PersistentState state) : base(_mapper, identity, state) { }

        public Namespace Namespace { set { Set<Namespace>(value, ref _namespace); } get { return Get<Namespace>(ref _namespace); } }
        /// <summary>
        /// Entity owning this request (can be null if owner is a namespace)
        /// </summary>
        public Entity Owner { set { Set<Entity>(value, ref owner); } get { return Get<Entity>(ref owner); } }
        /// <summary>
        /// JSON serialized abstract syntax tree of the query
        /// </summary>
        public string ParseTree { set { Set<string>(value, ref parseTree); } get { return Get<string>(ref parseTree); } }
        /// <summary>
        /// Data type of request (input data)
        /// </summary>
        public Entity RequestType { set { Set<Entity>(value, ref requestType); } get { return Get<Entity>(ref requestType); } }
        /// <summary>
        /// Data type of response (output data)
        /// </summary>
        public Entity ResponseType { set { Set<Entity>(value, ref responseType); } get { return Get<Entity>(ref responseType); } }
    }
}
