using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Zhichkin.Hermes.UI
{
    public class FunctionExpression : BindableBase
    {
        public FunctionExpression(TableExpression owner) { this.Owner = owner; }
        public TableExpression Owner { get; set; }
        public string Name { get; set; }
    }

    public enum BooleanOperatorType { AND, OR }
    public enum ComparisonOperatorType
    {
        Equal,
        NotEqual,
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual
    }

    public class BooleanExpression : ObservableCollection<object>
    {
        private BooleanOperatorType _ExpressionType = BooleanOperatorType.AND;

        public BooleanExpression() { }

        public BooleanOperatorType OperatorType
        {
            get { return _ExpressionType; }
            set
            {
                _ExpressionType = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("OperatorType"));
            }
        }

        public object Expression
        {
            get
            {
                if (this.Count == 1 && this[0] is BooleanOperator)
                {
                    return this[0];
                }
                else
                {
                    return this;
                }
            }
        }
    }

    public class BooleanOperator : BindableBase
    {
        private BooleanOperatorType _OperatorType = BooleanOperatorType.AND;

        public BooleanOperator()
        {
            this.Conditions = new ObservableCollection<ComparisonOperator>();
        }

        public BooleanOperatorType OperatorType
        {
            get { return _OperatorType; }
            set
            {
                _OperatorType = value;
                this.OnPropertyChanged("OperatorType");
            }
        }
        public ObservableCollection<ComparisonOperator> Conditions { get; set; }
    }

    public class ComparisonOperator : BindableBase
    {
        private PropertyExpression _LeftExpression;
        private ParameterExpression _RightExpression;
        private ComparisonOperatorType _OperatorType = ComparisonOperatorType.Equal;

        public ComparisonOperator() { }

        public ComparisonOperatorType OperatorType
        {
            get { return _OperatorType; }
            set
            {
                _OperatorType = value;
                this.OnPropertyChanged("OperatorType");
            }
        }
        public PropertyExpression LeftExpression
        {
            get { return _LeftExpression; }
            set
            {
                _LeftExpression = value;
                this.OnPropertyChanged("LeftExpression");
            }
        }
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
