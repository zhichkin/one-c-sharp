using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class ParameterReferenceViewModel : HermesViewModel
    {
        public ParameterReferenceViewModel(HermesViewModel parent, ParameterExpression model) : base(parent, model) { }
        public string Name { get { return ((ParameterExpression)this.Model).Name; } }
    }
}
