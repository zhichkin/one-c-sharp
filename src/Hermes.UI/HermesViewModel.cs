using Microsoft.Practices.Prism.Mvvm;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public abstract class HermesViewModel : BindableBase
    {
        public HermesViewModel(HermesViewModel parent) : base()
        {
            this.Parent = parent;
        }
        public HermesViewModel(HermesViewModel parent, HermesModel model) : this(parent)
        {
            this.Model = model;
        }
        public HermesModel Model { get; set; }
        public HermesViewModel Parent { get; set; }

        public QueryExpressionViewModel GetQueryExpressionViewModel(HermesViewModel child)
        {
            if (child is QueryExpressionViewModel) return (QueryExpressionViewModel)child;
            HermesViewModel parent = child.Parent;
            while (parent != null && !(parent is QueryExpressionViewModel))
            {
                parent = parent.Parent;
            }
            return (parent == null) ? null : (QueryExpressionViewModel)parent;
        }
        public SelectStatementViewModel GetSelectStatementViewModel(HermesViewModel child)
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
