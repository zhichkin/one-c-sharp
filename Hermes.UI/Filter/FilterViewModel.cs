using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Zhichkin.Hermes.UI
{
    // is used by where, on, case and having clauses
    public class FilterViewModel : BindableBase
    {
        public FilterViewModel()
        {
            this.BinaryExpressions = new ObservableCollection<BinaryExpression>();
        }
        public ObservableCollection<BinaryExpression> BinaryExpressions { get; private set; }
    }

    public class BinaryExpression : BindableBase
    {
        public BinaryExpression(string type) { this.ExpressionType = type; }
        public string ExpressionType { get; set; }
    }
    public class ConditionalExpression : BinaryExpression
    {
        public ConditionalExpression(string type) : base(type) { }
        public string LeftExpression { get; set; }
        ParameterViewModel _RightExpression;
        public ParameterViewModel RightExpression
        {
            get { return _RightExpression; }
            set
            {
                _RightExpression = value;
                this.OnPropertyChanged("RightExpression");
            }
        }
    }
    public class BinaryGroup : BinaryExpression
    {
        public BinaryGroup(string type) : base(type) { }
        public ObservableCollection<BinaryGroup> Children { get; set; }
        public ObservableCollection<ConditionalExpression> Conditions { get; set; }
    }
}
