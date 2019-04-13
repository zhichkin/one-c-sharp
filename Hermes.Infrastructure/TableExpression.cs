using System;
using System.Collections.Generic;
using System.Text;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.Model
{
    public class TableExpression
    {
        public TableExpression(Entity entity)
        {
            this.Entity = entity;
            this.Alias = this.Entity.Name;
        }
        public TableExpression(Entity entity, object consumer) : this(entity)
        {
            this.Consumer = consumer;
        }
        public string Name { get { return this.Entity.Name; } }
        public string Alias { get; set; }
        public Entity Entity { get; private set; }
        public object Consumer { get; set; } // null | TableExpression | Query | StoredProcedure | UserFunction
    }
    public class SelectStatement : TableExpression
    {
        public SelectStatement(Entity entity) : base(entity) { }
        public SelectStatement(Entity entity, object consumer) : base(entity, consumer) { }
        public BooleanFunction WHERE { get; set; }
        public BooleanFunction HAVING { get; set; }
        public List<TableExpression> FROM { get; set; }
        public List<PropertyExpression> SELECT { get; set; }
    }
}
