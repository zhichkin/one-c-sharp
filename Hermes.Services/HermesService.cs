using System;
using System.Collections;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.Services;
using Zhichkin.ORM;

namespace Zhichkin.Hermes.Services
{
    public interface IHermesService
    {
        string ToSQL(QueryExpression query);
        IEnumerable ExecuteQuery(QueryExpression query);
        Request GetTestRequest();
    }

    public sealed class HermesService : IHermesService
    {
        private const string CONST_TestRequestName = "TestRequest";

        public Request GetTestRequest()
        {
            Request request = null;

            IPersistentContext context = MetadataPersistentContext.Current;
            QueryService queryService = new QueryService(context.ConnectionString);
            string sql = $"SELECT [key] FROM [metadata].[requests] WHERE [namespace] = CAST(0x00000000000000000000000000000000 AS uniqueidentifier) AND [name] = N'{CONST_TestRequestName}';";
            object key = queryService.ExecuteScalar(sql);
            if (key == null)
            {
                IMetadataService metadata = new MetadataService();
                Namespace typeSystem = metadata.GetTypeSystemNamespace();

                request = new Request()
                {
                    Namespace = typeSystem,
                    Name = CONST_TestRequestName
                };
                request.Save();
            }
            else
            {
                request = context.Factory.New<Request>((Guid)key);
            }

            return request;
        }

        public string ToSQL(QueryExpression query)
        {
            QueryExecutor executor = new QueryExecutor(query);
            return executor.Build().ToSQL();
        }
        public IEnumerable ExecuteQuery(QueryExpression query)
        {
            QueryExecutor executor = new QueryExecutor(query);
            return executor.Build().Execute();
        }
    }
}
