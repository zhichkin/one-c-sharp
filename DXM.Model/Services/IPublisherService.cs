using Zhichkin.Metadata.Model;
using System.Collections.Generic;

namespace Zhichkin.DXM.Model
{
    public interface IPublisherService
    {
        Publication Create(InfoBase infoBase);
        List<Publication> Select(InfoBase infoBase);
        void Delete(Publication publication);

        PublicationProperty Create(Publication publication);
        List<PublicationProperty> Select(Publication publication);
        List<PublicationProperty> Select(Publication publication, PublicationPropertyPurpose purpose);
        void Delete(PublicationProperty property);

        Article Create(Publication publication, Entity entity);
        Article GetArticle(Publication publication, Entity entity);
        List<Article> GetArticles(Publication publication);
    }
}
