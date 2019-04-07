using Microsoft.Practices.Prism.Mvvm;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.UI
{
    public class PropertyExpressionViewModel : BindableBase
    {
        public PropertyExpressionViewModel(PropertyExpression model)
        {
            this.Model = model;
        }
        public PropertyExpression Model { get; private set; }
        public string Alias
        {
            get { return this.Model.Alias; }
            set
            {
                this.Model.Alias = value;
                OnPropertyChanged("Alias");
            }
        }
        public Property Property
        {
            get { return this.Model.Property; }
            set
            {
                this.Model.Property = value;
                OnPropertyChanged("Property");
            }
        }
    }
}
