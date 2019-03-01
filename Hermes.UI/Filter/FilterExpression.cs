using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Zhichkin.Hermes.UI
{
    // is used by where, on, case and having clauses
    public class FilterExpression : BindableBase
    {
        public FilterExpression(TableExpression owner)
        {
            this.Owner = owner;
            this.Children = new ObservableCollection<FilterExpression>();
            this.Conditions = new ObservableCollection<ConditionExpression>();
        }
        public string FilterType { get; set; }
        public TableExpression Owner { get; private set; }
        public ObservableCollection<FilterExpression> Children { get; private set; }
        public ObservableCollection<ConditionExpression> Conditions { get; private set; }
    }
    public class ConditionExpression : BindableBase
    {
        public ConditionExpression(FilterExpression owner) { this.Owner = owner; }
        public FilterExpression Owner { get; private set; }
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
