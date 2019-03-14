using System;
using System.Collections.Generic;
using System.Text;

namespace Zhichkin.Hermes.Infrastructure
{
    public interface IMetadataTreeNode
    {
        string Name { get; set; }
        IMetadataTreeNode Parent { get; set; }
        IList<IMetadataTreeNode> Children { get; }
        IMetadataInfo MetadataInfo { get; set; }
        IBooleanExpression Filter { get; set; }
        IFunctionExpression Function { get; set; }
    }
    public sealed class MetadataTreeNode : IMetadataTreeNode
    {
        public MetadataTreeNode()
        {
            this.Children = new List<IMetadataTreeNode>();
        }
        public string Name { get; set; }
        public IMetadataTreeNode Parent { get; set; }
        public IList<IMetadataTreeNode> Children { get; }
        public IMetadataInfo MetadataInfo { get; set; }
        public IBooleanExpression Filter { get; set; }
        public IFunctionExpression Function { get; set; }
        public int Count { get; set; }
        public List<Guid> Keys { get; set; }
    }
}