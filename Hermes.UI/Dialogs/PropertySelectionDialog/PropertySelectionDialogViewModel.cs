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

            SelectStatementViewModel select = GetSelectStatementViewModel(caller);
            if (select == null) throw new Exception("SelectStatementViewModel is not found!");

            // TODO: при поиске SELECT нужно учитывать, что в контексте предложения JOIN список Tables
            // должен ограничиться текущим JoinTableExpression, так как по правилам SQL "нижние"
            // по спику таблицы и их поля не видны в данной области видимости

            this.DialogItems.Clear();
            foreach(TableExpressionViewModel table in select.Tables)
            {
                this.DialogItems.Add(table);
            }
        }
        public HermesViewModel SelectedNode { get; set; }
        protected override object GetDialogResult()
        {
            if (this.SelectedNode != null && this.SelectedNode is TableExpressionViewModel) return null;
            return this.SelectedNode;
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
