using System;
using System.Collections.Generic;
using System.Text;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.Model
{
    public class PropertyReference : HermesModel
    {
        public PropertyReference(HermesModel consumer, TableExpression table, Property property) : base(consumer)
        {
            this.Table = table;
            this.Property = property;
        }
        public string Name { get { return this.Property.Name; } }
        public Property Property { get; private set; }
        public TableExpression Table { get; private set; }
    }
    public class PropertyExpression : HermesModel
    {
        public PropertyExpression(HermesModel consumer) : base(consumer) { }
        public string Alias { get; set; }
        public HermesModel Expression { get; set; }
    }
}
