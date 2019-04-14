using Microsoft.Practices.Prism.Mvvm;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class TableExpressionViewModel : HermesViewModel
    {
        public TableExpressionViewModel(HermesViewModel parent, TableExpression model) : base(parent, model) { }
        public string Name
        {
            get
            {
                if (this.Model == null) return string.Empty;
                return ((TableExpression)this.Model).Name;
            }
        }
        public string Alias
        {
            get
            {
                if (this.Model == null) return string.Empty;
                return ((TableExpression)this.Model).Alias;
            }
            set
            {
                if (this.Model == null) return; // TODO: ?
                ((TableExpression)this.Model).Alias = value;
                this.OnPropertyChanged("Alias");
            }
        }
    }
}
