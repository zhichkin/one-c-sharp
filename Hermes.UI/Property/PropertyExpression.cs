using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Zhichkin.Hermes.UI
{
    public class PropertyExpression : FunctionExpression
    {
        public PropertyExpression(TableExpression owner) : base(owner) { }
        public string Alias { get; set; }
        public FunctionExpression Function { get; set; }
        public string FullName { get { return string.Format("{0}.{1}", this.Owner.Alias, this.Name); } }

        // used to handle property selection from ComboBox
        public PropertyExpression PropertySelected
        {
            set
            {
                this.Name = value.Name;
                this.Alias = string.Empty;
                OnPropertyChanged("Alias");
                OnPropertyChanged("FullName");
            }
        }
        public ObservableCollection<FunctionExpression> AvailablePropertiesForSelection
        {
            get
            {
                return HermesUI.GetAvailablePropertiesForSelection(this.Owner);
            }
        }
    }
}
