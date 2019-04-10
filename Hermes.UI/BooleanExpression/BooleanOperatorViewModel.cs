using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class BooleanOperatorViewModel : BooleanFunctionViewModel
    {
        public BooleanOperatorViewModel(BooleanOperator model) : base(model) { }
        public ObservableCollection<BooleanFunctionViewModel> Operands { get; set; }
    }
}
