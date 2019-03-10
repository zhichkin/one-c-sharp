using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;
using Zhichkin.Hermes.Infrastructure;

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
        public INamespaceInfo Namespace { get; set; }
        public string FullName { get { return string.Format("{0}.{1}", this.Namespace?.Name, this.Name); } }
    }
}
