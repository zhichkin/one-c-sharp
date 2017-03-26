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

        public PublicationProperty Create(Publication publication)
        {
            PublicationProperty property = (PublicationProperty)DXMContext.Current.Factory.New(typeof(PublicationProperty));
            property.Owner = publication;
            return property;
        }
        public List<PublicationProperty> Select(Publication publication)
        {
            IPersistentContext context = DXMContext.Current;
            
            List<PublicationProperty> list = new List<PublicationProperty>();

            string sql = @"SELECT [key] FROM [dxm].[publication_properties] WHERE [publication] = @publication";

            using (SqlConnection connection = new SqlConnection(context.ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                command.CommandType = CommandType.Text;
                command.CommandText = sql;

                SqlParameter parameter = new SqlParameter("publication", SqlDbType.UniqueIdentifier)
                {
                    Direction = ParameterDirection.Input,
                    Value = (publication == null) ? Guid.Empty : publication.Identity
                };
                command.Parameters.Add(parameter);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(context.Factory.New<PublicationProperty>(reader.GetGuid(0)));
                }
            }

            return list;

        }
        public List<PublicationProperty> Select(Publication publication, PublicationPropertyPurpose purpose)
        {
            IPersistentContext context = DXMContext.Current;

            List<PublicationProperty> list = new List<PublicationProperty>();

            string sql = @"SELECT [key] FROM [dxm].[publication_properties] WHERE [publication] = @publication AND [purpose] = @purpose";

            using (SqlConnection connection = new SqlConnection(context.ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                command.CommandType = CommandType.Text;
                command.CommandText = sql;

                SqlParameter parameter = new SqlParameter("publication", SqlDbType.UniqueIdentifier)
                {
                    Direction = ParameterDirection.Input,
                    Value = (publication == null) ? Guid.Empty : publication.Identity
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter("purpose", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Input,
                    Value = (int)purpose
                };
                command.Parameters.Add(parameter);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(context.Factory.New<PublicationProperty>(reader.GetGuid(0)));
                }
            }

            return list;
        }
        public void Delete(PublicationProperty property)
        {
            property.Kill();
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
    }
}
