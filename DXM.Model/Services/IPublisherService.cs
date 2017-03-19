using Zhichkin.Metadata.Model;
using System.Collections.Generic;

namespace Zhichkin.DXM.Model
{
    public interface IPublisherService
    {
        Publication Create(InfoBase infoBase);
        List<Publication> Select(InfoBase infoBase);
        void Delete(Publication publication);
    }
}
