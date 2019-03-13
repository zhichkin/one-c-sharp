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
    public interface IBooleanTreeExpression : IBooleanExpression
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

    public class BooleanExpression : IBooleanTreeExpression
    {
        public BooleanExpression()
        {
            this.Conditions = new List<IComparisonExpression>();
        }
        public string Name { get; set; }
        public BooleanExpressionType ExpressionType { get; set; }
        public IBooleanExpression Parent { get; set; }
        public IList<IBooleanExpression> Children { get; set; }
        public IList<IComparisonExpression> Conditions { get; set; }
        public object Evaluate(params object[] parameters) { throw new NotImplementedException(); }
    }
    public class PropertyExpression : IPropertyExpression
    {
        public PropertyExpression() { }
        public string Name { get; set; }
        public IPropertyInfo PropertyInfo { get; set; }
        public object Evaluate(params object[] parameters) { throw new NotImplementedException(); }
    }

    public class ComparisonExpression : IComparisonExpression
    {
        public ComparisonExpression() { }
        public string Name { get; set; }
        public IFunctionExpression LeftExpression { get; set; }
        public IFunctionExpression RightExpression { get; set; }
        public BooleanExpressionType ExpressionType { get; set; }
        public object Evaluate(params object[] parameters) { throw new NotImplementedException(); }
    }
}
