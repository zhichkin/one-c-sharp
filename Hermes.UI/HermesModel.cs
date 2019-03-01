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

    public class FieldExpression
    {
        public FieldExpression(SelectExpression owner) { this.Owner = owner; }
        public string Alias { get; set; }
        public object Expression { get; set; }
        public SelectExpression Owner { get; private set; }
    }



    public class EntityExpression : BindableBase
    {
        public EntityExpression() { }
        public string Name { get; set; }
    }
    public class PropertyExpression : BindableBase
    {
        public PropertyExpression(EntityExpression owner) { this.Owner = owner; }
        public string Name { get; set; }
        public EntityExpression Owner { get; private set; }
    }
}
