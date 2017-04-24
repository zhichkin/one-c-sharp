using System.Collections.Generic;
using Zhichkin.Metadata.Model;
using Zhichkin.ORM;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Zhichkin.DXM.Model
{
    public sealed class PublisherService : IPublisherService
    {
        public Publication Create(InfoBase infoBase)
        {
            Publication publication = (Publication)DXMContext.Current.Factory.New(typeof(Publication));
            publication.Publisher = infoBase;
            return publication;
        }
        public List<Publication> Select(InfoBase infoBase)
        {
            List<Publication> list = new List<Publication>();
            IPersistentContext context = DXMContext.Current;
            using (SqlConnection connection = new SqlConnection(context.ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = @"SELECT [key] FROM [dxm].[publications] WHERE [publisher] = @publisher;";
                command.Parameters.AddWithValue("publisher", infoBase.Identity);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(context.Factory.New<Publication>(reader.GetGuid(0)));
                    }
                }
            }
            return list;
        }
        public void Delete(Publication publication)
        {
            publication.Kill();
        }
                
        public Article Create(Publication publication, Entity entity)
        {
            Article article = (Article)DXMContext.Current.Factory.New(typeof(Article));
            article.Name = entity.Name;
            article.Publication = publication;
            article.Entity = entity;
            article.Save();
            return article;
        }
        public Article GetArticle(Publication publication, Entity entity)
        {
            Article article = null;
            IPersistentContext context = DXMContext.Current;
            using (SqlConnection connection = new SqlConnection(context.ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = @"SELECT [key] FROM [dxm].[articles] WHERE [publication] = @publication AND [entity] = @entity;";
                command.Parameters.AddWithValue("publication", publication.Identity);
                command.Parameters.AddWithValue("entity", entity.Identity);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        article = context.Factory.New<Article>(reader.GetGuid(0));
                    }
                }
            }
            return article;
        }
        public List<Article> GetArticles(Publication publication)
        {
            List<Article> list = new List<Article>();
            IPersistentContext context = DXMContext.Current;
            using (SqlConnection connection = new SqlConnection(context.ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = @"SELECT [key] FROM [dxm].[articles] WHERE [publication] = @publication;";
                command.Parameters.AddWithValue("publication", publication.Identity);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(context.Factory.New<Article>(reader.GetGuid(0)));
                    }
                }
            }
            return list;
        }

        public ArticleFilter Create(Article article)
        {
            if (article == null) throw new ArgumentNullException("article");
            ArticleFilter filter = new ArticleFilter();
            filter.Article = article;
            filter.Property = article.Entity.Properties[0];
            filter.Value = null;
            return filter;
        }
        public List<ArticleFilter> Select(Article article)
        {
            return ArticleFilter.Select(article);
        }
        public void Delete(ArticleFilter filter)
        {
            filter.Kill();
        }

    }
}
