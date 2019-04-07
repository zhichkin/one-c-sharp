using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Windows.Input;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class BooleanExpressionViewModel : BindableBase
    {
        private int _ViewModelState = 0;

        public BooleanExpressionViewModel(object caller)
        {
            this.Caller = caller; // TableExpression(Model)
            this.AddNewOperatorCommand = new DelegateCommand(this.AddNewOperator);
            this.RemoveOperatorCommand = new DelegateCommand(this.RemoveOperator);
        }
        public ICommand AddNewOperatorCommand { get; private set; }
        public ICommand RemoveOperatorCommand { get; private set; }

        public object Caller { get; private set; }
        public object Expression { get; private set; }
        public int ViewModelState
        {
            get { return _ViewModelState; }
            set
            {
                _ViewModelState = value;
                this.OnPropertyChanged("ViewModelState");
            }
        }

        public void AddNewOperator()
        {
            if (this.Expression == null)
            {
                ComparisonOperator comparison = new ComparisonOperator(this.Caller);
                this.Expression = new ComparisonOperatorViewModel(comparison);
                this.ViewModelState = 1;
            }
            else if (this.Expression is ComparisonOperatorViewModel)
            {
                BooleanOperator caller = new BooleanOperator(this.Caller);
                ComparisonOperatorViewModel viewModel = (ComparisonOperatorViewModel)this.Expression;
                viewModel.Model.Caller = caller;
                caller.Operands.Add(viewModel.Model);
                caller.Operands.Add(new ComparisonOperator(caller));
                this.Expression = new BooleanOperatorViewModel(caller);
                this.ViewModelState = 2;
            }
            else if (this.Expression is BooleanOperatorViewModel)
            {
                BooleanOperatorViewModel viewModel = (BooleanOperatorViewModel)this.Expression;
                viewModel.Operands.Add(new ComparisonOperator(viewModel.Model));
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
        public void RemoveOperator()
        {
            if (this.ViewModelState == 2)
            {
                this.Expression = null;
                this.ViewModelState = 0;
            }
        }
    }
}
