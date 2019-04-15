using Microsoft.Practices.Prism.Commands;
using System.Collections.Generic;
using System.Windows.Input;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class ComparisonOperatorViewModel : BooleanFunctionViewModel
    {
        public ComparisonOperatorViewModel(HermesViewModel parent, ComparisonOperator model) : base(parent, model)
        {
            this.RemoveComparisonOperatorCommand = new DelegateCommand(this.RemoveComparisonOperator);
        }
        public HermesViewModel LeftExpression { get; set; } // ViewModel
        public HermesViewModel RightExpression { get; set; } // ViewModel
        public List<string> ComparisonOperators
        {
            get { return BooleanFunction.ComparisonOperators; }
        }

        public ICommand RemoveComparisonOperatorCommand { get; private set; }
        private void RemoveComparisonOperator()
        {
            ComparisonOperator model = this.Model as ComparisonOperator;
            if (model == null) return;

            if (model.IsRoot)
            {
                if (this.Parent is BooleanExpressionViewModel)
                {
                    ((BooleanExpressionViewModel)this.Parent).ClearBooleanExpression();
                }
            }
            else
            {
                BooleanOperatorViewModel parent = this.Parent as BooleanOperatorViewModel;
                if (parent == null) return;

                BooleanOperator consumer = model.Consumer as BooleanOperator;
                if (consumer == null) return;

                if (consumer.Operands.Count == 0) return;

                if (consumer.Operands.Count > 1)
                {
                    consumer.Operands.Remove(model);
                    parent.RemoveChildOperator(this);
                }
                else
                {
                    parent.RemoveBooleanOperatorCommand.Execute(null);
                }
            }
        }
    }
}
