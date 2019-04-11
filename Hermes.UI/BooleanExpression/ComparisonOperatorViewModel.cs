using System.Collections.Generic;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class ComparisonOperatorViewModel : BooleanFunctionViewModel
    {
        public ComparisonOperatorViewModel(ComparisonOperator model) : base(model) { }
        public object LeftExpression { get; set; } // ViewModel
        public object RightExpression { get; set; } // ViewModel
        public List<string> ComparisonOperators
        {
            get { return BooleanFunction.ComparisonOperators; }
        }
    }
}
