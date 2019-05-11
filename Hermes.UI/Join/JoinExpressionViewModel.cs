using System.Collections.ObjectModel;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class JoinExpressionViewModel : TableExpressionViewModel
    {
        public JoinExpressionViewModel(HermesViewModel parent, TableExpression model) : base(parent, model)
        {
            this.Filter = new BooleanExpressionViewModel(this, "ON");
        }
        public string JoinType
        {
            get
            {
                if (this.Model == null) return string.Empty;
                return ((JoinExpression)this.Model).JoinType;
            }
            set
            {
                if (this.Model == null) return;
                ((JoinExpression)this.Model).JoinType = value;
                this.OnPropertyChanged("JoinType");
            }
        }
        public TableExpressionViewModel Table
        {
            get
            {
                return (TableExpressionViewModel)this;
            }
        }
        public BooleanExpressionViewModel Filter { get; set; }
        public ObservableCollection<PropertyExpressionViewModel> Fields { get { return this.Fields; } }
    }
}
