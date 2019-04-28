using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;
using Zhichkin.Shell;

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
            if (this.Tables.Count == 0)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = "Предложение FROM не содержит ни одной таблицы!" });
                return;
            }
            PropertyExpression property = new PropertyExpression(this.Model);
            SelectStatement select = this.Model as SelectStatement;
            if (select.SELECT == null) { select.SELECT = new List<PropertyExpression>(); }
            select.SELECT.Add(property);
            this.Fields.Add(new PropertyExpressionViewModel(this, property));
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
                SelectStatement model = this.Model as SelectStatement;
                if (model.FROM == null)
                {
                    model.FROM = new List<TableExpression>();
                }
                TableExpression table = new TableExpression(model, entity);
                model.FROM.Add(table);
                TableExpressionViewModel viewModel = new TableExpressionViewModel(this, table);
                this.Tables.Add(viewModel);
            }
            // TODO: add JoinExpression
        }

        public void RemoveProperty(PropertyExpressionViewModel child)
        {
            this.Fields.Remove(child);

            SelectStatement select = this.Model as SelectStatement;
            if (select.SELECT == null || select.SELECT.Count == 0) return;

            PropertyExpression model = child.Model as PropertyExpression;
            if (model == null) return;
            select.SELECT.Remove(model);
        }
    }
}
