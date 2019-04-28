using System;
using System.Collections.ObjectModel;

namespace Zhichkin.Hermes.UI
{
    public class PropertySelectionDialogViewModel : DialogViewModelBase
    {
        public PropertySelectionDialogViewModel() : base()
        {
            this.DialogItems = new ObservableCollection<HermesViewModel>();
        }
        public ObservableCollection<HermesViewModel> DialogItems { get; private set; }
        protected override void InitializeViewModel(object input)
        {
            HermesViewModel caller = input as HermesViewModel;
            if (caller == null) throw new ArgumentOutOfRangeException();

            this.DialogItems.Clear();

            QueryExpressionViewModel query = GetQueryExpressionViewModel(caller);
            if (query == null) throw new Exception("QueryExpressionViewModel is not found!");
            this.DialogItems.Add(query);

            // TODO: при поиске SELECT нужно учитывать, что в контексте предложения JOIN список Tables
            // должен ограничиться текущим JoinTableExpression, так как по правилам SQL "нижние"
            // по спику таблицы и их поля не видны в данной области видимости
            SelectStatementViewModel select = GetSelectStatementViewModel(caller);
            if (select == null) throw new Exception("SelectStatementViewModel is not found!");
            foreach (TableExpressionViewModel table in select.Tables)
            {
                this.DialogItems.Add(table);
            }
        }
        public HermesViewModel SelectedNode { get; set; }
        protected override object GetDialogResult()
        {
            if (this.SelectedNode != null
                &&
                (this.SelectedNode is TableExpressionViewModel
                || this.SelectedNode is QueryExpressionViewModel)) { return null; }

            if (this.SelectedNode is ParameterExpressionViewModel)
            {
                return ((ParameterExpressionViewModel)this.SelectedNode).GetParameterReferenceViewModel(this.SelectedNode.Parent);
            }
            return this.SelectedNode;
        }

        private QueryExpressionViewModel GetQueryExpressionViewModel(HermesViewModel child)
        {
            if (child is QueryExpressionViewModel) return (QueryExpressionViewModel)child;
            HermesViewModel parent = child.Parent;
            while (parent != null && !(parent is QueryExpressionViewModel))
            {
                parent = parent.Parent;
            }
            return (parent == null) ? null : (QueryExpressionViewModel)parent;
        }

        private SelectStatementViewModel GetSelectStatementViewModel(HermesViewModel child)
        {
            if (child is SelectStatementViewModel) return (SelectStatementViewModel)child;
            HermesViewModel parent = child.Parent;
            while (parent != null && !(parent is SelectStatementViewModel))
            {
                parent = parent.Parent;
            }
            return (parent == null) ? null : (SelectStatementViewModel)parent;
        }
    }
}
