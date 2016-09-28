using System;
using System.Data;
using System.Collections.Generic;

namespace Zhichkin.ORM
{
    public static class Helper
    {
        private static readonly Dictionary<string, SqlDbType> typesLookup = new Dictionary<string, SqlDbType>()
        {
            { "bigint", SqlDbType.BigInt },
            { "image", SqlDbType.Binary },
            { "bit", SqlDbType.Bit },
            { "char", SqlDbType.Char },
            { "date", SqlDbType.Date },
            { "datetime", SqlDbType.DateTime },
            { "smalldatetime", SqlDbType.DateTime },
            { "datetime2", SqlDbType.DateTime2 },
            { "datetimeoffset", SqlDbType.DateTimeOffset },
            { "decimal", SqlDbType.Decimal },
            { "numeric", SqlDbType.Decimal },
            { "float", SqlDbType.Float },
            { "int", SqlDbType.Int },
            { "money", SqlDbType.Money },
            { "nchar", SqlDbType.NChar },
            { "ntext", SqlDbType.NText },
            { "nvarchar", SqlDbType.NVarChar },
            { "real", SqlDbType.Real },
            { "smallint", SqlDbType.SmallInt },
            { "smallmoney", SqlDbType.SmallMoney },
            { "text", SqlDbType.Text },
            { "time", SqlDbType.Time },
            { "rowversion", SqlDbType.Timestamp },
            { "timestamp", SqlDbType.Timestamp },
            { "tinyint", SqlDbType.TinyInt },
            { "uniqueidentifier", SqlDbType.UniqueIdentifier },
            { "binary", SqlDbType.VarBinary },
            { "varbinary", SqlDbType.VarBinary },
            { "varbinary(max)", SqlDbType.VarBinary },
            { "varchar", SqlDbType.VarChar },
            { "sql_variant", SqlDbType.Variant },
            { "xml", SqlDbType.Xml }
        };
        public static SqlDbType GetSqlTypeByName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) throw new ArgumentNullException("typeName");
            return typesLookup[typeName];
        }
    }
}
