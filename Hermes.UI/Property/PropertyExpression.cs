using Microsoft.Practices.Prism.Mvvm;

namespace Zhichkin.Hermes.UI
{
    public class PropertyExpression : BindableBase
    {
        public PropertyExpression(TableExpression owner) { this.Owner = owner; }
        public TableExpression Owner { get; private set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public object Expression { get; set; }
    }
}
