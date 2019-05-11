using System;
using System.Collections.Generic;
using System.Text;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.Model
{
    public static class JoinTypes
    {
        public static string InnerJoin = "INNER JOIN";
        public static string LeftJoin = "LEFT JOIN";
        public static string RightJoin = "RIGHT JOIN";
        public static string FullJoin = "FULL JOIN";
    }

    public class TableExpression : HermesModel
    {
        public TableExpression(HermesModel consumer, Entity entity) : base(consumer)
        {
            this.Consumer = consumer;
            this.Entity = entity;
            if (this.Entity != null) // newly created SelectStatement can nave no entity yet
            {
                this.Alias = this.Entity.Name;
            }
        }
        public string Name { get { return this.Entity.Name; } }
        public string Alias { get; set; }
        public Entity Entity { get; private set; }
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
