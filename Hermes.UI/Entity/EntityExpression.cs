using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Zhichkin.Hermes.UI
{
    public class TableExpression : BindableBase
    {
        public TableExpression() { }
        public string Alias { get; set; }
    }

    public class EntityExpression : TableExpression
    {
        public EntityExpression(TableExpression owner) { this.Owner = owner; }
        public TableExpression Owner { get; private set; }
        public string Name { get; set; }
    }
}
