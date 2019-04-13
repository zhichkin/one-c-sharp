using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class JoinExpressionViewModel : TableExpressionViewModel
    {
        public JoinExpressionViewModel(TableExpression model) : base(model)
        {
            this.Filter = new BooleanExpressionViewModel(this, "ON");
        }
        public string JoinType { get; set; }
        public TableExpression Table { get; set; }
        public BooleanExpressionViewModel Filter { get; set; }
        public ObservableCollection<PropertyExpressionViewModel> Fields { get { return this.Fields; } }
    }
}
