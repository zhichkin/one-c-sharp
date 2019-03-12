using System;
using System.Collections.Generic;
using System.Text;

namespace Zhichkin.Hermes.Infrastructure
{
    public interface IDataTreeNode
    {
        IDataTreeNode Parent { get; set; }
        IList<IDataTreeNode> Children { get; }
        IEntityInfo EntityInfo { get; set; }
        IBooleanExpression Filter { get; set; }
        IFunctionExpression Function { get; set; }
    }
    public sealed class DataTreeNode : IDataTreeNode
    {
        public DataTreeNode()
        {
            this.Children = new List<IDataTreeNode>();
        }
        public IDataTreeNode Parent { get; set; }
        public IList<IDataTreeNode> Children { get; }
        public IEntityInfo EntityInfo { get; set; }
        public IBooleanExpression Filter { get; set; }
        public IFunctionExpression Function { get; set; }
    }
}