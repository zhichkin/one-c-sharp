using Microsoft.Practices.Prism.Mvvm;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.UI
{
    public class PropertyExpressionViewModel : HermesViewModel
    {
        public PropertyExpressionViewModel(HermesViewModel parent, PropertyExpression model) : base(parent, model) { }
        public string Alias
        {
            get
            {
                if (this.Model == null) return string.Empty;
                return ((PropertyExpression)this.Model).Alias;
            }
            set
            {
                if (this.Model == null) return;
                ((PropertyExpression)this.Model).Alias = value;
                OnPropertyChanged("Alias");
            }
        }
    }
}
