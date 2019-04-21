using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class ComparisonOperatorViewModel : BooleanFunctionViewModel
    {
        private UserControl _LeftExpressionView;
        private UserControl _RightExpressionView;

        public ComparisonOperatorViewModel(HermesViewModel parent, ComparisonOperator model) : base(parent, model)
        {
            this.RemoveComparisonOperatorCommand = new DelegateCommand(this.RemoveComparisonOperator);

            this.PropertySelectionDialog = new InteractionRequest<Confirmation>();
            this.OpenPropertySelectionDialogCommand = new DelegateCommand<string>(this.OpenPropertySelectionDialog);
        }
        public List<string> ComparisonOperators
        {
            get { return BooleanFunction.ComparisonOperators; }
        }
        public UserControl LeftExpressionView
        {
            get { return _LeftExpressionView; }
            set
            {
                _LeftExpressionView = value;
                this.OnPropertyChanged("LeftExpressionView");
            }
        }
        public UserControl RightExpressionView
        {
            get { return _RightExpressionView; }
            set
            {
                _RightExpressionView = value;
                this.OnPropertyChanged("RightExpressionView");
            }
        }
        public HermesViewModel LeftExpression { get; set; } // ViewModel
        public HermesViewModel RightExpression { get; set; } // ViewModel
        
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

        public ICommand OpenPropertySelectionDialogCommand { get; private set; }
        public InteractionRequest<Confirmation> PropertySelectionDialog { get; private set; }
        private void OpenPropertySelectionDialog(string parameter)
        {
            Confirmation confirmation = new Confirmation() { Title = "Select property", Content = this };
            this.PropertySelectionDialog.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    if (parameter == "LEFT")
                    {
                        OnLeftExpressionSelected((HermesViewModel)response.Content);
                    }
                    else if (parameter == "RIGHT")
                    {
                        OnRightExpressionSelected((HermesViewModel)response.Content);
                    }
                }
            });
        }

        private void OnLeftExpressionSelected(HermesViewModel selectedExpression)
        {
            if (selectedExpression == null)
            {
                this.LeftExpression = null;
                this.LeftExpressionView = null;
            }

            PropertyReferenceViewModel viewModel = selectedExpression as PropertyReferenceViewModel;
            if (selectedExpression == null) return;

            ComparisonOperator model = (ComparisonOperator)this.Model;
            model.LeftExpression = selectedExpression.Model;
            model.LeftExpression.Consumer = model;

            this.LeftExpression = selectedExpression;
            this.LeftExpression.Parent = this;
            this.LeftExpressionView = new PropertyReferenceView((PropertyReferenceViewModel)this.LeftExpression);
        }
        private void OnRightExpressionSelected(HermesViewModel selectedExpression)
        {
            if (selectedExpression == null)
            {
                this.RightExpression = null;
                this.RightExpressionView = null;
            }

            PropertyReferenceViewModel viewModel = selectedExpression as PropertyReferenceViewModel;
            if (selectedExpression == null) return;

            ComparisonOperator model = (ComparisonOperator)this.Model;
            model.RightExpression = selectedExpression.Model;
            model.RightExpression.Consumer = model;

            this.RightExpression = selectedExpression;
            this.RightExpression.Parent = this;
            this.RightExpressionView = new PropertyReferenceView((PropertyReferenceViewModel)this.RightExpression);
        }
    }
}
