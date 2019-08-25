using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using Zhichkin.Hermes.Model;
using Zhichkin.Shell;

namespace Zhichkin.Hermes.UI
{
    public class BooleanExpressionViewModel : HermesViewModel
    {
        private UserControl _View;
        private BooleanFunction _Model;
        private bool _IsCommandPanelVisible = true;

        public BooleanExpressionViewModel(HermesViewModel parent, string clause) : base(parent)
        {
            if (parent == null) throw new ArgumentNullException();
            this.Clause = clause;
            GetModelFromParent();

            this.InitializeViewModel();

            this.AddNewConditionCommand = new DelegateCommand(this.AddNewCondition);
        }
        private void InitializeViewModel()
        {
            if (_Model is BooleanOperator)
            {
                this.IsCommandPanelVisible = false;
                this.View = new BooleanOperatorView(new BooleanOperatorViewModel(this, (BooleanOperator)_Model));
            }
            else if (_Model is ComparisonOperator)
            {
                this.IsCommandPanelVisible = true;
                this.View = new ComparisonOperatorView(new ComparisonOperatorViewModel(this, (ComparisonOperator)_Model));
            }
        }

        private void GetModelFromParent()
        {
            if (this.Parent is SelectStatementViewModel)
            {
                SelectStatement model = this.Parent.Model as SelectStatement;
                if (this.Clause == "WHERE")
                {
                    if (model != null) { _Model = model.WHERE; }
                }
                else if (this.Clause == "HAVING")
                {
                    if (model != null) { _Model = model.HAVING; }
                }
            }
            else if (this.Parent is JoinExpressionViewModel)
            {
                JoinExpression model = this.Parent.Model as JoinExpression;
                if (this.Clause == "ON")
                {
                    if (model != null) { _Model = model.ON; }
                }
            }
        }
        private void SetModelToParent()
        {
            if (this.Parent is SelectStatementViewModel)
            {
                SelectStatement model = this.Parent.Model as SelectStatement;
                if (this.Clause == "WHERE")
                {
                    if (model != null) { model.WHERE = _Model; }
                }
                else if (this.Clause == "HAVING")
                {
                    if (model != null) { model.HAVING = _Model; }
                }
            }
            else if (this.Parent is JoinExpressionViewModel)
            {
                JoinExpression model = this.Parent.Model as JoinExpression;
                if (this.Clause == "ON")
                {
                    if (model != null) { model.ON = _Model; }
                }
            }
        }
        public string Clause { get; private set; }
        public UserControl View
        {
            get { return _View; }
            set
            {
                _View = value;
                this.OnPropertyChanged("View");
            }
        }

        public bool IsCommandPanelVisible
        {
            get { return _IsCommandPanelVisible; }
            set
            {
                _IsCommandPanelVisible = value;
                this.OnPropertyChanged("IsCommandPanelVisible");
            }
        }

        public ICommand AddNewConditionCommand { get; private set; }
        private void AddNewCondition()
        {
            if (this.Parent is SelectStatementViewModel)
            {
                if (((SelectStatementViewModel)this.Parent).Tables.Count == 0)
                {
                    Z.Notify(new Notification { Title = "Hermes", Content = "Предложение FROM не содержит ни одной таблицы!" });
                    return;
                }
            }

            if (_Model == null)
            {
                _Model = new ComparisonOperator(this.Parent.Model);
                SetModelToParent();
                ComparisonOperatorViewModel viewModel = new ComparisonOperatorViewModel(this, (ComparisonOperator)_Model);
                this.View = new ComparisonOperatorView(viewModel);
            }
            else if (_Model is ComparisonOperator)
            {
                ComparisonOperatorViewModel currentVM = this.View.DataContext as ComparisonOperatorViewModel;

                BooleanOperator substitute = new BooleanOperator(this.Parent.Model);
                substitute.AddChild(_Model);
                BooleanOperatorViewModel substituteVM = new BooleanOperatorViewModel(this, substitute);

                ComparisonOperator child = new ComparisonOperator(substitute);
                substitute.AddChild(child);
                ComparisonOperatorViewModel childVM = new ComparisonOperatorViewModel(substituteVM, child);

                currentVM.Parent = substituteVM;
                substituteVM.Operands = new ObservableCollection<BooleanFunctionViewModel>() { currentVM, childVM };

                BooleanOperatorView substituteView = new BooleanOperatorView(substituteVM);

                _Model = substitute;
                SetModelToParent();
                this.IsCommandPanelVisible = false;
                this.View = substituteView;
            }
        }
        public void ClearBooleanExpression()
        {
            _Model = null;
            SetModelToParent();
            this.View = null;
            this.IsCommandPanelVisible = true;
        }
        public void SetBooleanExpression(BooleanFunctionViewModel vm)
        {
            _Model = (BooleanFunction)vm.Model;
            _Model.Consumer = this.Parent.Model;
            vm.Parent = this;
            SetModelToParent();
            if (_Model is ComparisonOperator)
            {
                this.IsCommandPanelVisible = true;
                this.View = new ComparisonOperatorView((ComparisonOperatorViewModel)vm);
            }
            else if (_Model is BooleanOperator)
            {
                this.IsCommandPanelVisible = false;
                this.View = new BooleanOperatorView((BooleanOperatorViewModel)vm);
            }
        }
    }
}
