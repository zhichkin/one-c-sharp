using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;
using Zhichkin.Hermes.Infrastructure;

namespace Zhichkin.Hermes.UI
{
    public class EntityInfo : IEntityInfo
    {
        public EntityInfo(int typeCode) { this.TypeCode = typeCode; }
        public int TypeCode { get; private set; }
        public string Name { get; set; }
    }
}
