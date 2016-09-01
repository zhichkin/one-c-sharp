using System;

using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public partial class Namespace
    {
        public static readonly Namespace TypeSystem;

        static Namespace()
        {
            QueryService service = new QueryService(MetadataPersistentContext.Current.ConnectionString);
            string sql = "SELECT [key] FROM [namespaces] WHERE [key] = CAST(0x00000000000000000000000000000000 AS uniqueidentifier);";
            object key = service.ExecuteScalar(sql);
            TypeSystem = new Namespace((Guid)key, PersistentState.Virtual);
            TypeSystem.Load();
        }
    }
}
