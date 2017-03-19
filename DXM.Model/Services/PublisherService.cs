using System.Collections.Generic;
using Zhichkin.Metadata.Model;
using Zhichkin.ORM;
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
    }
}
