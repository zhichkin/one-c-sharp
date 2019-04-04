using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

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

    public class BooleanExpression : BindableBase
    {
        private object _Expression;
        private int _ViewModelState = 0;

        public BooleanExpression()
        {
            this.AddComparisonOperatorCommand = new DelegateCommand(this.AddComparisonOperator);
            this.RemoveComparisonOperatorCommand = new DelegateCommand(this.RemoveComparisonOperator);
        }
        public ICommand AddComparisonOperatorCommand { get; private set; }
        public ICommand RemoveComparisonOperatorCommand { get; private set; }

        public object Parent { get; set; }
        public object Expression
        {
            get { return _Expression; }
            set
            {
                _Expression = value;
                this.OnPropertyChanged("Expression");
            }
        }
        public int ViewModelState
        {
            get { return _ViewModelState; }
            set
            {
                _ViewModelState = value;
                this.OnPropertyChanged("ViewModelState");
            }
        }

        public void AddComparisonOperator()
        {
            if (this.Expression == null)
            {
                this.Expression = new ComparisonOperator() { Parent = this };
                this.ViewModelState = 1;
            }
            else if (this.Expression is ComparisonOperator)
            {
                BooleanOperator parent = new BooleanOperator() { Parent = this };
                parent.Conditions.Add((ComparisonOperator)this.Expression);
                this.Expression = parent;
                this.ViewModelState = 2;
            }
            else if (this.Expression is BooleanOperator)
            {
                this.ViewModelState = 3;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
        public void RemoveComparisonOperator()
        {
            if (this.ViewModelState == 2)
            {
                this.Expression = null;
                this.ViewModelState = 0;
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

        public object Parent { get; set; }
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
        private object _LeftExpression;
        private object _RightExpression;
        private ComparisonOperatorType _OperatorType = ComparisonOperatorType.Equal;

        public ComparisonOperator() { }

        public object Parent { get; set; }
        public ComparisonOperatorType OperatorType
        {
            get { return _OperatorType; }
            set
            {
                _OperatorType = value;
                this.OnPropertyChanged("OperatorType");
            }
        }
        public object LeftExpression
        {
            get { return _LeftExpression; }
            set
            {
                _LeftExpression = value;
                this.OnPropertyChanged("LeftExpression");
            }
        }
        public object RightExpression
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
