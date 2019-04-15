using Microsoft.Practices.Prism.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class BooleanOperatorViewModel : BooleanFunctionViewModel
    {
        public BooleanOperatorViewModel(HermesViewModel parent, BooleanOperator model) : base(parent, model)
        {
            this.ShowOperands(); // recursive call to this constructor ... аднака =)

            this.AddComparisonOperatorCommand = new DelegateCommand(this.AddComparisonOperator);
            this.AddInnerBooleanOperatorCommand = new DelegateCommand(this.AddInnerBooleanOperator);
            this.AddOuterBooleanOperatorCommand = new DelegateCommand(this.AddOuterBooleanOperator);
            this.RemoveBooleanOperatorCommand = new DelegateCommand(this.RemoveBooleanOperator);
        }
        public ICommand AddComparisonOperatorCommand { get; private set; }
        public ICommand AddInnerBooleanOperatorCommand { get; private set; }
        public ICommand AddOuterBooleanOperatorCommand { get; private set; }
        public ICommand RemoveBooleanOperatorCommand { get; private set; }

        public ObservableCollection<BooleanFunctionViewModel> Operands { get; set; }
        public List<string> BooleanOperators { get { return BooleanFunction.BooleanOperators; } }

        public void RemoveChildOperator(BooleanFunctionViewModel child)
        {
            this.Operands.Remove(child);
        }
        public void ShowOperands()
        {
            if (this.Operands == null)
            {
                this.Operands = new ObservableCollection<BooleanFunctionViewModel>();
            }
            else
            {
                this.Operands.Clear();
            }

            BooleanOperator model = this.Model as BooleanOperator;
            if (model == null || model.Operands == null) return;

            foreach (BooleanFunction operand in model.Operands)
            {
                if (operand is ComparisonOperator)
                {
                    this.Operands.Add(new ComparisonOperatorViewModel(this, (ComparisonOperator)operand));
                }
                else if (operand is BooleanOperator)
                {
                    this.Operands.Add(new BooleanOperatorViewModel(this, (BooleanOperator)operand));
                }
            }
        }

        private void AddComparisonOperator()
        {
            BooleanOperator model = this.Model as BooleanOperator;
            if (model == null) return;
            if (!model.IsLeaf) return;

            ComparisonOperator operand = new ComparisonOperator(model);
            model.AddChild(operand);
            ComparisonOperatorViewModel viewModel = new ComparisonOperatorViewModel(this, operand);
            this.Operands.Add(viewModel);
        }
        private void AddInnerBooleanOperator()
        {
            BooleanOperator model = this.Model as BooleanOperator;
            if (model == null) return;

            if (model.IsLeaf)
            {
                AddOuterBooleanOperator();
            }
            else
            {
                BooleanOperator operand = new BooleanOperator(model) { Name = BooleanFunction.OR };
                model.AddChild(operand);
                operand.AddChild(new ComparisonOperator(operand));
                BooleanOperatorViewModel viewModel = new BooleanOperatorViewModel(this, operand);
                this.Operands.Add(viewModel);
            }
        }
        private void AddOuterBooleanOperator()
        {
            BooleanOperator model = this.Model as BooleanOperator;
            if (model == null) return;

            BooleanOperator clone = new BooleanOperator(model) { Name = model.Name };
            foreach (BooleanFunction operand in model.Operands)
            {
                clone.AddChild(operand);
            }
            model.Operands = new List<BooleanFunction>();

            BooleanOperator child = new BooleanOperator(model) { Name = BooleanFunction.OR };
            child.AddChild(new ComparisonOperator(child));

            model.AddChild(clone);
            model.AddChild(child);
            this.Operands.Clear();
            foreach (BooleanFunction operand in model.Operands)
            {
                this.Operands.Add(new BooleanOperatorViewModel(this, (BooleanOperator)operand));
            }
        }
        private void RemoveBooleanOperator()
        {
            BooleanOperator model = this.Model as BooleanOperator;
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

                if (consumer.Operands.Count > 2)
                {
                    consumer.Operands.Remove(model);
                    parent.RemoveChildOperator(this);
                }
                else if (consumer.Operands.Count == 2)
                {
                    consumer.Operands.Remove(model);
                    BooleanOperator orphan = consumer.Operands[0] as BooleanOperator;
                    consumer.Operands.Clear();
                    foreach (BooleanFunction operand in orphan.Operands)
                    {
                        consumer.AddChild(operand);
                    }
                    if (consumer.Name == BooleanFunction.AND && orphan.Name == BooleanFunction.OR)
                    {
                        parent.Name = BooleanFunction.OR; // this will also set consumer.Name property
                    }
                    parent.ShowOperands();
                }
                else // (consumer.Operands.Count == 1) it shouldn't be like this
                {
                    consumer.Operands.Remove(model);
                    parent.RemoveChildOperator(this);
                }
            }
        }
    }
}