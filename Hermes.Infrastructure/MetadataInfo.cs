using System;
using System.Collections.Generic;
using System.Text;

namespace Zhichkin.Hermes.Infrastructure
{
    public class EntityInfo : IEntityInfo
    {
        public EntityInfo() { }
        public EntityInfo(Guid identity)
        {
            this.Identity = identity;
        }
        public EntityInfo(int typeCode)
        {
            this.TypeCode = typeCode;
        }
        public Guid Identity { get; private set; }
        public int TypeCode { get; set; }
        public string Name { get; set; }
        public INamespaceInfo Namespace { get; set; }
        public string FullName { get; set; }
        public IList<IPropertyInfo> Properties { get; set; }
        public IEntityInfo Owner { get; set; }
        public IList<ITableInfo> Tables { get; }
    }
}
