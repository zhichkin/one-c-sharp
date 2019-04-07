using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Unity;
using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.Commands;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class SelectExpression : TableExpression
    {
        public SelectExpression()
        {
            this.Tables = new ObservableCollection<TableExpression>();
            this.WhereClause = new BooleanExpressionViewModel(this) { };
            this.AddPropertyCommand = new DelegateCommand(this.OnAddProperty);
        }
        public ICommand AddPropertyCommand { get; private set; }
        private void OnAddProperty()
        {
            string alias = "Поле_" + this.Fields.Count.ToString();
            PropertyExpression property = new PropertyExpression() { Alias = alias };
            this.Fields.Add(new PropertyExpressionViewModel(property));
        }

        private bool _IsFromVertical = true;
        private string _FromClauseDescription = "Tabular data source names ...";
        public bool IsFromVertical
        {
            get { return _IsFromVertical; }
            set
            {
                _IsFromVertical = value;
                this.OnPropertyChanged("IsFromVertical");
            }
        }
        public string FromClauseDescription
        {
            get { return _FromClauseDescription; }
            private set
            {
                _FromClauseDescription = value;
                this.OnPropertyChanged("FromClauseDescription");
            }
        }
        public ObservableCollection<TableExpression> Tables { get; set; }
        public BooleanExpressionViewModel WhereClause { get; set; }
    }
}
