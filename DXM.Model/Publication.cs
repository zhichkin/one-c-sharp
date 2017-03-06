using System;
using System.Collections.Generic;
using Zhichkin.ORM;
using Zhichkin.Metadata.Model;

namespace Zhichkin.DXM.Model
{
    public sealed partial class Publication : EntityBase
    {
        private static readonly IDataMapper _mapper = DXMContext.Current.GetDataMapper(typeof(Publication));
        public static Publication Create(InfoBase infoBase)
        {
            Publication publication = (Publication)DXMContext.Current.Factory.New(typeof(Publication));
            publication.Publisher = infoBase;
            return publication;
        }
        public static IList<Publication> Select(InfoBase infoBase) { return DataMapper.Select(infoBase); }

        private InfoBase _Publisher = null;

        public Publication() : base(_mapper) { }
        public Publication(Guid identity) : base(_mapper, identity) { }
        public Publication(Guid identity, PersistentState state) : base(_mapper, identity, state) { }
        
        public InfoBase Publisher { set { Set<InfoBase>(value, ref _Publisher); } get { return Get<InfoBase>(ref _Publisher); } }
    }
}
