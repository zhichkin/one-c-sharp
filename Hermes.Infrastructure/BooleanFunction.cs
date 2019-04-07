using System;
using System.Collections.Generic;
using System.Text;

namespace Zhichkin.Hermes.Model
{
    public class BooleanFunction
    {
        #region " Predefined names "

        private const string CONST_AND = "AND";
        private const string CONST_OR = "OR";
        private const string CONST_NOT = "NOT";
        private const string CONST_Equal = "=";
        private const string CONST_NotEqual = "<>";
        private const string CONST_Greater = ">";
        private const string CONST_GreaterOrEqual = ">=";
        private const string CONST_Less = "<";
        private const string CONST_LessOrEqual = "<=";

        public static string AND { get { return CONST_AND; } }
        public static string OR { get { return CONST_OR; } }
        public static string NOT { get { return CONST_NOT; } }
        public static string Equal { get { return CONST_Equal; } }
        public static string NotEqual { get { return CONST_NotEqual; } }
        public static string Greater { get { return CONST_Greater; } }
        public static string GreaterOrEqual { get { return CONST_GreaterOrEqual; } }
        public static string Less { get { return CONST_Less; } }
        public static string LessOrEqual { get { return CONST_LessOrEqual; } }

        #endregion

        public BooleanFunction(object caller)
        {
            this.Caller = caller;
        }
        public string Name { get; set; }
        public object Caller { get; set; }
    }
    
    public class BooleanOperator : BooleanFunction
    {
        public BooleanOperator(object caller) : base(caller)
        {
            this.Name = BooleanFunction.AND;
        }
        public List<BooleanFunction> Operands { get; set; } = new List<BooleanFunction>();
    }
    public class ComparisonOperator : BooleanFunction
    {
        public ComparisonOperator(object caller) : base(caller)
        {
            this.Name = BooleanFunction.Equal;
        }
        public object LeftExpression { get; set; }
        public object RightExpression { get; set; }
    }
}
