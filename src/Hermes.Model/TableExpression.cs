using System.Collections.Generic;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.Model
{
    public static class JoinTypes
    {
        public static string InnerJoin = "INNER JOIN";
        public static string LeftJoin = "LEFT JOIN";
        public static string RightJoin = "RIGHT JOIN";
        public static string FullJoin = "FULL JOIN";
        public static List<string> JoinTypesList = new List<string>()
        {
            JoinTypes.InnerJoin,
            JoinTypes.LeftJoin,
            JoinTypes.RightJoin,
            JoinTypes.FullJoin
        };
    }

    public static class HintTypes
    {
        public static string NoneHint = "NONE HINT";
        public static string ReadUncommited = "READUNCOMMITTED";
        public static string ReadCommited = "READCOMMITTED";
        public static string ReadCommitedLock = "READCOMMITTEDLOCK";
        public static string RepeatableRead = "REPEATABLEREAD";
        public static string Serializable = "SERIALIZABLE";
        public static string UpdateLock = "UPDLOCK";
        public static string ReadPast = "READPAST";
        public static string RowLock = "ROWLOCK";
        public static List<string> HintTypesList = new List<string>()
        {
            HintTypes.NoneHint,
            HintTypes.ReadUncommited,
            HintTypes.ReadCommited,
            HintTypes.ReadCommitedLock,
            HintTypes.RepeatableRead,
            HintTypes.Serializable,
            HintTypes.UpdateLock,
            HintTypes.ReadPast,
            HintTypes.RowLock
        };
    }

    public class TableExpression : HermesModel
    {
        public TableExpression(HermesModel consumer) : base(consumer)
        {
            this.Hint = HintTypes.NoneHint;
        }
        public TableExpression(HermesModel consumer, Entity entity) : this(consumer)
        {
            this.Entity = entity;
            if (this.Entity != null) // newly created SelectStatement can nave no entity yet
            {
                this.Alias = this.Entity.Name;
            }
        }
        public string Name { get { return (this.Entity == null)? string.Empty: this.Entity.Name; } }
        public string Alias { get; set; }
        public Entity Entity { get; set; }
        public string Hint { get; set; }
    }
    public class SelectStatement : TableExpression
    {
        public SelectStatement(HermesModel consumer, Entity entity) : base(consumer, entity) { }
        public BooleanFunction WHERE { get; set; }
        public BooleanFunction HAVING { get; set; }
        public List<TableExpression> FROM { get; set; }
        public List<PropertyExpression> SELECT { get; set; }
    }
    public class JoinExpression : TableExpression
    {
        public JoinExpression(HermesModel consumer, Entity entity) : base(consumer, entity)
        {
            this.JoinType = JoinTypes.InnerJoin;
        }
        public string JoinType { get; set; }
        public BooleanFunction ON { get; set; }
        // TODO: can be transformed into SelectStatement
    }
}
