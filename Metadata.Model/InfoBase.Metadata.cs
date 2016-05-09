using System;
using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public partial class InfoBase
    {
        public static readonly InfoBase Metadata;
        static InfoBase()
        {
            QueryService service = new QueryService(PersistentContext.ConnectionString);
            string sql = "SELECT [key] FROM [infobases] WHERE [key] = CAST(0x00000000000000000000000000000000 AS uniqueidentifier);";
            object key = service.ExecuteScalar(sql);
            Metadata = new InfoBase((Guid)key, PersistentState.Virtual);
            Metadata.Load();
        }
    }
}
