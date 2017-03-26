using System;
using Zhichkin.Metadata.Model;
using Zhichkin.ORM;

namespace Zhichkin.DXM.Model
{
    public enum PublicationPropertyPurpose
    {
        /// <summary>
        /// Property is used to filter change tracking data for registration (publication level)
        /// </summary>
        Filtration,
        /// <summary>
        /// Property is used to route data messages containing change tracking data (subscriber level)
        /// </summary>
        Routing
    }

    public sealed partial class PublicationProperty : EntityBase
    {
        private static readonly IDataMapper _mapper = DXMContext.Current.GetDataMapper(typeof(PublicationProperty));

        private Publication _owner = null;
        private Entity _type = null;
        private byte[] _value = new byte[16]; // 256 max
        private PublicationPropertyPurpose _purpose = PublicationPropertyPurpose.Filtration;

        public PublicationProperty() : base(_mapper) { }
        public PublicationProperty(Guid identity) : base(_mapper, identity) { }
        public PublicationProperty(Guid identity, PersistentState state) : base(_mapper, identity, state) { }

        public Publication Owner { set { Set<Publication>(value, ref _owner); } get { return Get<Publication>(ref _owner); } }
        public Entity Type { set { Set<Entity>(value, ref _type); } get { return Get<Entity>(ref _type); } }
        public byte[] Value { set { Set<byte[]>(value, ref _value); } get { return Get<byte[]>(ref _value); } }
        public PublicationPropertyPurpose Purpose { set { Set<PublicationPropertyPurpose>(value, ref _purpose); } get { return Get<PublicationPropertyPurpose>(ref _purpose); } }
    }
}
