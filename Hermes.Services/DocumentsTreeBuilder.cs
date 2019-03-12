using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Zhichkin.Hermes.Infrastructure;

namespace Zhichkin.Hermes.Services
{
    public sealed class DocumentsTreeService
    {
        private const string CONST_ModuleName = "Zhichkin.Metadata";
        private readonly string CONST_ConnectionString;
        public DocumentsTreeService()
        {
            CONST_ConnectionString = ConfigurationManager.ConnectionStrings[CONST_ModuleName].ConnectionString;
        }

        public IDataTreeNode BuildDocumentsTree(IEntityInfo document)
        {
            DataTreeNode root = new DataTreeNode() { EntityInfo = document };

            FillChildren(root);

            return root;
        }
        private void FillChildren(DataTreeNode parent)
        {
            IList<IEntityInfo> children = GetChildren(parent.EntityInfo);
            foreach (IEntityInfo child in children)
            {
                DataTreeNode node = new DataTreeNode()
                {
                    Parent = parent,
                    EntityInfo = child
                };
                parent.Children.Add(node);
                FillChildren(node);
            }
        }
        private IList<IEntityInfo> GetChildren(IEntityInfo parent)
        {
            List<IEntityInfo> list = new List<IEntityInfo>();

            // проверка цикличности ссылок !!!
            //parent.Identity

            using (SqlConnection connection = new SqlConnection(CONST_ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = @"SELECT [property] FROM [metadata].[relations] WHERE [entity] = @entity;";
                command.Parameters.AddWithValue("entity", parent.Identity);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //list.Add(context.Factory.New<Publication>(reader.GetGuid(0)));
                    }
                }
            }

            return list;
        }
    }
}
