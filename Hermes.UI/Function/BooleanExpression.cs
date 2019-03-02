using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Zhichkin.Hermes.UI
{
    public class FunctionExpression : BindableBase
    {
        public FunctionExpression(TableExpression owner) { this.Owner = owner; }
        public TableExpression Owner { get; private set; }
        public string Name { get; set; }
    }
    
    public class BooleanExpression : FunctionExpression
    {
        public BooleanExpression(TableExpression owner) : base(owner)
        {
            this.Children = new ObservableCollection<BooleanExpression>();
            this.Conditions = new ObservableCollection<ComparisonExpression>();
        }
        public string FilterType { get; set; }
        public ObservableCollection<BooleanExpression> Children { get; private set; }
        public ObservableCollection<ComparisonExpression> Conditions { get; private set; }
    }
    public class ComparisonExpression : FunctionExpression
    {
        public ComparisonExpression(TableExpression owner) : base(owner) { }
        public string LeftExpression { get; set; }
        ParameterExpression _RightExpression;
        public ParameterExpression RightExpression
        {
            get { return _RightExpression; }
            set
            {
                _RightExpression = value;
                this.OnPropertyChanged("RightExpression");
            }
        }
    }
}
