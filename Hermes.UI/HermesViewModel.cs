using Microsoft.Practices.Prism.Mvvm;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public abstract class HermesViewModel : BindableBase
    {
        public HermesViewModel(HermesViewModel parent) : base()
        {
            this.Parent = parent;
        }
        public HermesViewModel(HermesViewModel parent, HermesModel model) : this(parent)
        {
            this.Model = model;
        }
        public HermesModel Model { get; set; }
        public HermesViewModel Parent { get; set; }
    }
}
