using System.Collections.Generic;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class ComparisonOperatorViewModel : BooleanFunctionViewModel
    {
        public ComparisonOperatorViewModel(HermesViewModel parent, ComparisonOperator model) : base(parent, model) { }
        public HermesViewModel LeftExpression { get; set; } // ViewModel
        public HermesViewModel RightExpression { get; set; } // ViewModel
        public List<string> ComparisonOperators
        {
            get { return BooleanFunction.ComparisonOperators; }
        }
    }
}
