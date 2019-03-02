using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;
using Zhichkin.Hermes.Infrastructure;

namespace Zhichkin.Hermes.UI
{
    public class TypeInfo : ITypeInfo
    {
        public int Code { get; set; }
        public string Name { get; set; }
    }
}
