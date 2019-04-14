using System.Collections.Generic;
using System.Collections.ObjectModel;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class BooleanOperatorViewModel : BooleanFunctionViewModel
    {
        public BooleanOperatorViewModel(HermesViewModel parent, BooleanOperator model) : base(parent, model) { }
        public ObservableCollection<BooleanFunctionViewModel> Operands { get; set; }
        public List<string> BooleanOperators { get { return BooleanFunction.BooleanOperators; } }
    }
}
