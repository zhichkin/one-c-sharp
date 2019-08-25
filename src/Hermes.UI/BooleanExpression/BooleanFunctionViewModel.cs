using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class BooleanFunctionViewModel : HermesViewModel
    {
        public BooleanFunctionViewModel(HermesViewModel parent, BooleanFunction model) : base(parent, model) { }
        public string Name
        {
            get { return ((BooleanFunction)this.Model).Name; }
            set
            {
                ((BooleanFunction)this.Model).Name = value;
                this.OnPropertyChanged("Name");
            }
        }
    }
}
