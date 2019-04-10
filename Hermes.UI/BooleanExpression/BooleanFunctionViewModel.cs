using Microsoft.Practices.Prism.Mvvm;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class BooleanFunctionViewModel : BindableBase
    {
        public BooleanFunctionViewModel(BooleanFunction model)
        {
            this.Model = model;
        }
        public object Parent { get; set; } // Parent ViewModel
        public BooleanFunction Model { get; private set; }
        public string Name
        {
            get { return this.Model.Name; }
            set
            {
                this.Model.Name = value;
                this.OnPropertyChanged("Name");
            }
        }
    }
}
