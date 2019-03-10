using System;
using System.Collections.Generic;
using System.Text;

namespace Zhichkin.Hermes.Infrastructure
{
    public interface IInfoBaseInfo
    {
        string Name { set; get; }
    }
    public interface INamespaceInfo
    {
        string Name { get; set; }
        IInfoBaseInfo InfoBase { get; }
        INamespaceInfo Namespace { get; }
    }
    public interface IEntityInfo
    {
        int TypeCode { get; }
        string Name { get; set; }
        INamespaceInfo Namespace { get; set; }
        string FullName { get; }
        IList<IPropertyInfo> Properties { get; }
    }
    public interface IPropertyInfo
    {
        string Name { get; set; }
        IEntityInfo Entity { get; set; }
    }
}
