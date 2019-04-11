using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Windows.Input;
using Zhichkin.Hermes.Model;
using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public class BooleanExpressionViewModel : BindableBase
    {
        private UserControl _View;
        private BooleanFunction _Model;
        private BooleanExpressionViewBuilder _ViewBuilder = new BooleanExpressionViewBuilder();
        private bool _IsCommandPanelVisible = true;

        public BooleanExpressionViewModel(object caller, string clause)
        {
            this.Caller = caller; // Model of type TableExpression
            // TODO: initialize _Model with the value of caller's property referenced by clause argument !
            this.Clause = clause;
            this.AddNewConditionCommand = new DelegateCommand(this.AddNewCondition);
        }
        public object Caller { get; private set; }
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
            if (_Model == null)
            {
                _Model = new ComparisonOperator(this.Caller);
                // TODO: set caller's clause property to _Model
                this.View = _ViewBuilder.Build(this, _Model);
            }
            else if (_Model is ComparisonOperator)
            {
                BooleanOperator bo = new BooleanOperator(this.Caller);
                bo.AddChild(_Model);
                bo.AddChild(new ComparisonOperator(bo));
                _Model = bo;
                this.IsCommandPanelVisible = false;
                this.View = _ViewBuilder.Build(this, _Model);
            }
        }
    }
}
