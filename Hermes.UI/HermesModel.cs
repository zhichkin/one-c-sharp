using Microsoft.Practices.Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Zhichkin.Hermes.Infrastructure;

namespace Zhichkin.Hermes.UI
{
    public class EntityInfo : IEntityInfo
    {
        public EntityInfo(int typeCode) { this.TypeCode = typeCode; }
        public int TypeCode { get; private set; }
        public string Name { get; set; }
        public INamespaceInfo Namespace { get; set; }
        public string FullName { get; set; }
        public IList<IPropertyInfo> Properties { get; set; }
    }
}
