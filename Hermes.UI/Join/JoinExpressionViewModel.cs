using System.Collections.Generic;
using System.Collections.ObjectModel;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.UI
{
    public class JoinExpressionViewModel : TableExpressionViewModel
    {
        public JoinExpressionViewModel(HermesViewModel parent, TableExpression model) : base(parent, model)
        {
            this.Filter = new BooleanExpressionViewModel(this, "ON");
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

        public string SelectedJoinTypeItem
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
                this.OnPropertyChanged("SelectedJoinTypeItem");
            }
        }
        public List<string> JoinTypeItemsSource { get { return JoinTypes.JoinTypesList; } }
    }
}
