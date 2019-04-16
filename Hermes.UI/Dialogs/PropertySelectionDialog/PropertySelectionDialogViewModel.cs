using System;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class PropertySelectionDialogViewModel : DialogViewModelBase
    {
        public PropertySelectionDialogViewModel() : base() { }
        protected override void InitializeViewModel(object input)
        {
            PropertyExpressionViewModel caller = input as PropertyExpressionViewModel;
            PropertyExpression model = caller.Model as PropertyExpression;
            BooleanExpressionViewModel boolexVM = GetBooleanExpressionViewModel(caller);
            //TableExpression table;
            string clause = boolexVM.Clause;
        }
        protected override object GetDialogResult()
        {
            PropertyExpression output = null;



            return output;
        }

        private BooleanExpressionViewModel GetBooleanExpressionViewModel(HermesViewModel child)
        {
            if (child is BooleanExpressionViewModel) return (BooleanExpressionViewModel)child;
            HermesViewModel parent = child.Parent;
            while (parent != null && !(parent is BooleanExpressionViewModel))
            {
                parent = parent.Parent;
            }
            return (parent == null) ? null : (BooleanExpressionViewModel)parent;
        }
    }
}
