using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.Model
{
    public class PropertyReference : HermesModel
    {
        public PropertyReference(HermesModel consumer) : base(consumer) { }
        public PropertyReference(HermesModel consumer, TableExpression table, Property property) : this(consumer)
        {
            this.Table = table;
            this.Property = property;
        }
        public string Name { get { return this.Property.Name; } }
        public Property Property { get; set; }
        public TableExpression Table { get; set; }
    }
    public class PropertyExpression : HermesModel
    {
        public PropertyExpression(HermesModel consumer) : base(consumer) { }
        public string Alias { get; set; }
        public HermesModel Expression { get; set; }
    }
}
