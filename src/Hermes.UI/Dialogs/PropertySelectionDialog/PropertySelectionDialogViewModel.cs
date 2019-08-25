using System;
using System.Collections.ObjectModel;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class PropertySelectionDialogViewModel : DialogViewModelBase
    {
        public PropertySelectionDialogViewModel() : base()
        {
            this.DialogItems = new ObservableCollection<HermesViewModel>();
        }
        public ObservableCollection<HermesViewModel> DialogItems { get; private set; }
        protected override void InitializeViewModel()
        {
            if (this.Caller == null) throw new ArgumentOutOfRangeException("Caller");

            this.DialogItems.Clear();

            QueryExpressionViewModel query = this.Caller.GetQueryExpressionViewModel(this.Caller);
            if (query == null) throw new Exception("QueryExpressionViewModel is not found!");
            this.DialogItems.Add(query);

            // TODO: при поиске SELECT нужно учитывать, что в контексте предложения JOIN список Tables
            // должен ограничиться текущим JoinTableExpression, так как по правилам SQL "нижние"
            // по спику таблицы и их поля не видны в данной области видимости
            SelectStatementViewModel select = this.Caller.GetSelectStatementViewModel(this.Caller);
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

            HermesViewModel parentVM = this.Caller as ComparisonOperatorViewModel;
            if (parentVM == null)
            {
                parentVM = this.Caller as PropertyExpressionViewModel;
            }
            PropertyReferenceViewModel selectedVM = this.SelectedNode as PropertyReferenceViewModel;
            PropertyReference source = selectedVM.Model as PropertyReference;
            if (parentVM != null && selectedVM != null && source != null)
            {
                PropertyReference model = new PropertyReference(parentVM.Model, source.Table, source.Property);
                return new PropertyReferenceViewModel(parentVM, selectedVM.Table, model);
            }
            else
            {
                return null;
            }
        }
    }
}
