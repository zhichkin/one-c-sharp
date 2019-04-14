using System;
using System.Collections.Generic;
using System.Text;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.Model
{
    public class TableExpression : HermesModel
    {
        public TableExpression(HermesModel consumer, Entity entity) : base(consumer)
        {
            this.Consumer = consumer;
            this.Entity = entity;
            this.Alias = this.Entity.Name;
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
}
