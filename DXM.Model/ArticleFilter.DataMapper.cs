using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Zhichkin.Metadata.Model;
using Zhichkin.ORM;

namespace Zhichkin.DXM.Model
{
    public partial class ArticleFilter
    {
        public sealed class DataMapper : IDataMapper
        {
            # region " SQL "
            private const string InsertCommandText =
                @"INSERT [dxm].[article_filters] ([article], [property], [operator], [type], [value]) VALUES (@article, @property, @operator, @type, @value); SELECT @@ROWCOUNT;";
            private const string UpdateCommandText =
                @"UPDATE [dxm].[article_filters]" +
                @" SET [article] = @article_new, [property] = @property_new, [operator] = @operator, [type] = @type, [value] = @value" +
                @" WHERE [article] = @article_old AND [property] = @property_old; " +
                @"SELECT @@ROWCOUNT;";
            private const string DeleteCommandText =
                @"DELETE [dxm].[article_filters] WHERE [article] = @article_old AND [property] = @property_old; SELECT @@ROWCOUNT;";
            #endregion

            private readonly string ConnectionString;
            private readonly IReferenceObjectFactory Factory;
            private DataMapper() { }
            public DataMapper(string connectionString, IReferenceObjectFactory factory)
            {
                ConnectionString = connectionString;
                Factory = factory;
            }
            void IDataMapper.Select(IPersistent entity)
            {
                throw new NotSupportedException();
            }
            void IDataMapper.Insert(IPersistent entity)
            {
                ArticleFilter e = (ArticleFilter)entity;
                IBinaryFormatter formatter = new BinaryFormatter();

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command  = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = InsertCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("article", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e._article.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("property", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e._property.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("operator", SqlDbType.Int);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (int)e._operator;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("type", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e._type == null) ? Guid.Empty : e._type.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("value", SqlDbType.VarBinary);
                    parameter.Direction = ParameterDirection.Input;
                    MemoryStream stream = new MemoryStream();
                    formatter.Serialize(stream, e._value);
                    parameter.Value = stream.ToArray();
                    command.Parameters.Add(parameter);

                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read()) { ok = (int)reader[0] > 0; }
                    reader.Close(); connection.Close();
                }

                if (!ok) throw new ApplicationException("Error executing insert command.");
            }
            void IDataMapper.Update(IPersistent entity)
            {
                ArticleFilter e = (ArticleFilter)entity;
                IBinaryFormatter formatter = new BinaryFormatter();

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command  = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = UpdateCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("article_new", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e._article.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("article_old", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e._article_old.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("property_new", SqlDbType.Timestamp);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e._property.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("property_old", SqlDbType.Timestamp);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e._property_old.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("operator", SqlDbType.Int);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (int)e._operator;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("type", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = (e._type == null) ? Guid.Empty : e._type.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("value", SqlDbType.VarBinary);
                    parameter.Direction = ParameterDirection.Input;
                    MemoryStream stream = new MemoryStream();
                    formatter.Serialize(stream, e._value);
                    parameter.Value = stream.ToArray();
                    command.Parameters.Add(parameter);

                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read()) { ok = (int)reader[0] > 0; }
                    reader.Close(); connection.Close();
                }
                if (!ok) throw new ApplicationException("Error executing update command.");
            }
            void IDataMapper.Delete(IPersistent entity)
            {
                ArticleFilter e = (ArticleFilter)entity;

                bool ok = false;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = DeleteCommandText;

                    SqlParameter parameter = null;

                    parameter = new SqlParameter("article_old", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e._article_old.Identity;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("property_old", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = e._property_old.Identity;
                    command.Parameters.Add(parameter);

                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read()) { ok = (int)reader[0] > 0; }
                    reader.Close(); connection.Close();
                }
                if (!ok) throw new ApplicationException("Error executing delete command.");
            }

            public static List<ArticleFilter> Select(Article article)
            {
                if (article == null) throw new ArgumentNullException("article");
                IBinaryFormatter formatter = new BinaryFormatter();
                List<ArticleFilter> list = new List<ArticleFilter>();
                IPersistentContext context = DXMContext.Current;
                using (SqlConnection connection = new SqlConnection(context.ConnectionString))
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"SELECT [article], [property], [operator], [type], [value] FROM [dxm].[article_filters] WHERE [article] = @article;";
                    SqlParameter parameter = new SqlParameter("article", SqlDbType.UniqueIdentifier);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = article.Identity;
                    command.Parameters.Add(parameter);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ArticleFilter entity = new ArticleFilter(PersistentState.Loading);
                            entity._article = context.Factory.New<Article>(reader.GetGuid(0));
                            entity._property = MetadataPersistentContext.Current.Factory.New<Property>(reader.GetGuid(1));
                            entity._operator = (FilterOperator)reader.GetInt32(2);
                            entity._type = MetadataPersistentContext.Current.Factory.New<Entity>(reader.GetGuid(3));
                            entity._value = formatter.Deserialize(new MemoryStream((byte[])reader[4]), entity._type);
                            entity.State = PersistentState.Original;
                            list.Add(entity);
                        }
                    }
                }
                return list;
            }
        }
    }
}
