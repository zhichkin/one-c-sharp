using System;
using System.Collections.Generic;
using System.Text;

namespace Zhichkin.Hermes.Infrastructure
{
    public enum BooleanExpressionType
    {
        OR,
        AND,
        Equal,
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual,
        NotEqual
    }

    public interface IFunctionExpression
    {
        string Name { get; set; }
        object Evaluate(params object[] parameters);
    }

    public interface IPropertyExpression : IFunctionExpression
    {
        IPropertyInfo PropertyInfo { get; set; }
    }

    public interface IBooleanExpression : IFunctionExpression
    {
        BooleanExpressionType ExpressionType { get; set; }
    }
    public interface IBooleanGroupExpression : IBooleanExpression
    {
        IBooleanExpression Parent { get; set; }
        IList<IBooleanExpression> Children { get; set; }
        IList<IComparisonExpression> Conditions { get; set; }
    }
    public interface IComparisonExpression : IBooleanExpression
    {
        IFunctionExpression LeftExpression { get; set; }
        IFunctionExpression RightExpression { get; set; }
    }
}
