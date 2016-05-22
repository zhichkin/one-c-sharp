using System;
using Zhichkin.ORM;
using System.Collections.Generic;

namespace Zhichkin.Metadata.Model
{
    public sealed partial class InfoBase : EntityBase
    {
        private static readonly IDataMapper _mapper = MetadataPersistentContext.Current.GetDataMapper(typeof(InfoBase));

        public InfoBase() : base(_mapper) { }
        public InfoBase(Guid identity) : base(_mapper, identity) { }
        public InfoBase(Guid identity, PersistentState state) : base(_mapper, identity, state) { }

        private string server = string.Empty;
        private string database = string.Empty;

        public string Server { set { Set<string>(value, ref server); } get { return Get<string>(ref server); } }
        public string Database { set { Set<string>(value, ref database); } get { return Get<string>(ref database); } }

        public List<Namespace> Namespaces { set; get; }
    }
}
