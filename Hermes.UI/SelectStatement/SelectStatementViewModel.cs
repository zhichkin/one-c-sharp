using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Unity;
using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.Commands;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;
using System.Collections.Generic;

namespace Zhichkin.Hermes.UI
{
    public class SelectStatementViewModel : TableExpressionViewModel
    {
        public SelectStatementViewModel(HermesViewModel parent, SelectStatement model) : base(parent, model)
        {
            this.Tables = new ObservableCollection<TableExpressionViewModel>();
            this.Fields = new ObservableCollection<PropertyExpressionViewModel>();
            this.WhereClause = new BooleanExpressionViewModel(this, "WHERE");
            this.AddTableCommand = new DelegateCommand<Entity>(this.OnAddTable);
            this.AddPropertyCommand = new DelegateCommand(this.OnAddProperty);
        }
        public ICommand AddTableCommand { get; private set; }
        public ICommand AddPropertyCommand { get; private set; }
        private void OnAddProperty()
        {
            this.Fields.Add(new PropertyExpressionViewModel(this, null));
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
        public ObservableCollection<TableExpressionViewModel> Tables { get; set; }
        public BooleanExpressionViewModel WhereClause { get; set; }
        public ObservableCollection<PropertyExpressionViewModel> Fields { get; set; }

        private void OnAddTable(Entity entity)
        {
            if (this.Model == null)
            {
                SelectStatement model = new SelectStatement(null, entity);
                model.FROM = new List<TableExpression>();
                TableExpression table = new TableExpression(model, entity);
                model.FROM.Add(table);
                TableExpressionViewModel viewModel = new TableExpressionViewModel(this, table);
                this.Model = model;
                this.Tables.Add(viewModel);
                this.WhereClause = new BooleanExpressionViewModel(this, "WHERE");
            }
            else
            {
                // TODO: add JoinExpression
            }
        }
    }
}
