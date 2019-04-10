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

        }
        private void GetView()
        {
            this.View = _ViewBuilder.Build(_Model);
        }
    }
}
