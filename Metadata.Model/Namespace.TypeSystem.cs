using System;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using System.Collections.Generic;

using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public partial class Namespace
    {
        public static readonly Namespace TypeSystem;

        static Namespace()
        {
            QueryService service = new QueryService(PersistentContext.ConnectionString);
            string sql = "SELECT [key] FROM [namespaces] WHERE [key] = CAST(0x00000000000000000000000000000000 AS uniqueidentifier);";
            object key = service.ExecuteScalar(sql);
            TypeSystem = new Namespace((Guid)key, PersistentState.Virtual);
            TypeSystem.Load();
        }
    }
}
