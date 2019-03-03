using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Zhichkin.Hermes.UI
{
    public class JoinExpression : TableExpression
    {
        public JoinExpression(TableExpression owner) : base(owner)
        {
            this.Filter = new BooleanExpression(this);
        }
        public string JoinType { get; set; }
        public TableExpression Table { get; set; }
        public BooleanExpression Filter { get; set; }
        public override ObservableCollection<PropertyExpression> Fields { get { return this.Table.Fields; } }
    }
}
