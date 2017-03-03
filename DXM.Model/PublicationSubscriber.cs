using System;
using Zhichkin.ORM;
using Zhichkin.Metadata.Model;

namespace Zhichkin.DXM.Model
{
    public sealed partial class PublicationSubscriber : ValueObject
    {
        private static readonly IDataMapper _mapper = DXMContext.Current.GetDataMapper(typeof(PublicationSubscriber));

        private Publication _Publication = null;
        private Publication _Old_Publication = null;
        private InfoBase _Subscriber = null;
        private InfoBase _Old_Subscriber = null;

        public PublicationSubscriber() : base(_mapper) { }
        public PublicationSubscriber(PersistentState state) : base(_mapper, state) { }

        public Publication Publication { set { Set<Publication>(value, ref _Publication); } get { return Get<Publication>(ref _Publication); } }
        public InfoBase Subscriber { set { Set<InfoBase>(value, ref _Subscriber); } get { return Get<InfoBase>(ref _Subscriber); } }

        protected override void UpdateKeyValues()
        {
            _Old_Publication = _Publication;
            _Old_Subscriber = _Subscriber;
        }
    }
}
