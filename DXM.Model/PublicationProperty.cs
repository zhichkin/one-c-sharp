using System;
using System.IO;
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
        private Entity _type = Entity.Empty;
        private object _value = null;
        private PublicationPropertyPurpose _purpose = PublicationPropertyPurpose.Filtration;

        public PublicationProperty() : base(_mapper) { }
        public PublicationProperty(Guid identity) : base(_mapper, identity) { }
        public PublicationProperty(Guid identity, PersistentState state) : base(_mapper, identity, state) { }

        public Publication Owner { set { Set<Publication>(value, ref _owner); } get { return Get<Publication>(ref _owner); } }
        public PublicationPropertyPurpose Purpose { set { Set<PublicationPropertyPurpose>(value, ref _purpose); } get { return Get<PublicationPropertyPurpose>(ref _purpose); } }
        public Entity Type
        {
            get { return Get<Entity>(ref _type); }
            private set
            {
                if (value == null)
                {
                    Set<Entity>(Entity.Empty, ref _type);
                }
                else
                {
                    Set<Entity>(value, ref _type);
                }
            }
        }
        public object Value
        {
            get { return Get<object>(ref _value); }
            set
            {
                if (value == null)
                {
                    this.Type = Entity.Empty;
                }
                else
                {
                    Entity metadata = Entity.GetMetadataType(value.GetType());
                    if (metadata == Entity.Object)
                    {
                        metadata = ((ReferenceProxy)value).Type; // !!! ReferenceProxy !!!
                    }
                    if (metadata != _type)
                    {
                        this.Type = metadata;
                    }
                }
                Set<object>(value, ref _value);
            }
        }
    }
}
