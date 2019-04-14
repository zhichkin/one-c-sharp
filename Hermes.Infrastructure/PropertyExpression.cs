using System;
using System.Collections.Generic;
using System.Text;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.Model
{
    public class PropertyExpression : HermesModel
    {
        public PropertyExpression(HermesModel consumer, Property property) : base(consumer)
        {
            this.Property = property;
            this.Alias = this.Property.Name;
        }
        public string Name { get { return this.Property.Name; } }
        public string Alias { get; set; }
        public Property Property { get; private set; }
    }
}
