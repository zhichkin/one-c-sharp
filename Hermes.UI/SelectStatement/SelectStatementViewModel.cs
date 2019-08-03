using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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

            this.InitializeViewModel(model);

            this.AddTableCommand = new DelegateCommand<Entity>(this.OnAddTable);
            this.AddPropertyCommand = new DelegateCommand(this.OnAddProperty);
        }
        private void InitializeViewModel(SelectStatement model)
        {
            if (model.FROM != null && model.FROM.Count > 0)
            {
                foreach (TableExpression table in model.FROM)
                {
                    this.AddTable(table);
                }
            }

            if (model.SELECT != null && model.SELECT.Count > 0)
            {
                foreach (PropertyExpression property in model.SELECT)
                {
                    this.AddProperty(property);
                }
            }

            if (model.WHERE != null)
            {
                this.WhereClause.Model = model.WHERE;
            }
        }
        private void AddTable(TableExpression table)
        {
            if (table is JoinExpression)
            {
                this.Tables.Add(new JoinExpressionViewModel(this, table));
            }
            else if (table is SelectStatement)
            {
                this.Tables.Add(new SelectStatementViewModel(this, (SelectStatement)table));
            }
            else
            {
                this.Tables.Add(new TableExpressionViewModel(this, table));
            }
        }
        private void AddProperty(PropertyExpression property)
        {
            this.Fields.Add(new PropertyExpressionViewModel(this, property));
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
        private string _FromClauseDescription;
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
            get
            {
                _FromClauseDescription = "Tabular data source names ...";
                SelectStatement model = this.Model as SelectStatement;
                if (model == null) return _FromClauseDescription;
                if (model.FROM == null) return _FromClauseDescription;
                if (model.FROM.Count == 0) return _FromClauseDescription;

                StringBuilder names = new StringBuilder();
                foreach (TableExpression table in model.FROM)
                {
                    if (names.Length > 0)
                    {
                        names.Append(", ");
                    }
                    int length = names.Length + table.Alias.Length;
                    if (length > 75)
                    {
                        names.Append(table.Alias.Substring(0, 10));
                        names.Append(" ...");
                        break;
                    }
                    else
                    {
                        names.Append(table.Alias);
                    }
                }
                _FromClauseDescription = names.ToString();

                return _FromClauseDescription;
            }
        }
        public ObservableCollection<TableExpressionViewModel> Tables { get; set; }
        public BooleanExpressionViewModel WhereClause { get; set; }
        public ObservableCollection<PropertyExpressionViewModel> Fields { get; set; }

        private void OnAddTable(Entity entity)
        {
            if (this.Model == null)
            {
                SelectStatement select = new SelectStatement(null, entity);
                select.FROM = new List<TableExpression>();
                TableExpression first_table = new TableExpression(select, entity);
                select.FROM.Add(first_table);
                TableExpressionViewModel vm = new TableExpressionViewModel(this, first_table);
                this.Model = select;
                this.Tables.Add(vm);
                this.WhereClause = new BooleanExpressionViewModel(this, "WHERE");
                this.OnPropertyChanged("FromClauseDescription");
                return;
            }

            SelectStatement model = this.Model as SelectStatement;
            if (model.FROM == null) { model.FROM = new List<TableExpression>(); }

            if (model.FROM.Count == 0)
            {
                TableExpression table = new TableExpression(model, entity);
                model.FROM.Add(table);
                TableExpressionViewModel tableVM = new TableExpressionViewModel(this, table);
                this.Tables.Add(tableVM);
            }
            else
            {
                JoinExpression join = new JoinExpression(model, entity);
                model.FROM.Add(join);
                JoinExpressionViewModel joinVM = new JoinExpressionViewModel(this, join);
                this.Tables.Add(joinVM);
            }
            this.OnPropertyChanged("FromClauseDescription");
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
