using Zhichkin.Metadata.Model;
using System.Collections.Generic;

namespace Zhichkin.DXM.Model
{
    public interface IPublisherService
    {
        Publication Create(InfoBase infoBase);
        List<Publication> Select(InfoBase infoBase);
        void Delete(Publication publication);

        Article Create(Publication publication, Entity entity);
        Article GetArticle(Publication publication, Entity entity);
        List<Article> GetArticles(Publication publication);

        ArticleFilter Create(Article article);
        List<ArticleFilter> Select(Article article);
        void Delete(ArticleFilter filter);
    }
}
