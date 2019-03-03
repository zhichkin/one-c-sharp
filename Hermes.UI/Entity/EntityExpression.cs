using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Zhichkin.Hermes.UI
{
    public class TableExpression : BindableBase
    {
        public TableExpression(TableExpression owner)
        {
            this.Owner = owner;
            this.Fields = new ObservableCollection<PropertyExpression>();
        }
        public TableExpression Owner { get; private set; }
        public string Alias { get; set; }
        public virtual ObservableCollection<PropertyExpression> Fields { get; set; }
    }

    public class EntityExpression : TableExpression
    {
        public EntityExpression(TableExpression owner) : base(owner) { }
        public string Name { get; set; }
        public override ObservableCollection<PropertyExpression> Fields { get { return HermesUI.GetTestEntityFields(this); } }
    }
}
