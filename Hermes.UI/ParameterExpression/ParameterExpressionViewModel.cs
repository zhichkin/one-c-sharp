using Microsoft.Practices.Prism.Mvvm;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.UI
{
    public class ParameterExpressionViewModel : HermesViewModel
    {
        private string _Name;
        private Entity _Type;
        private object _Value;

        public ParameterExpressionViewModel(HermesViewModel parent) : base(parent) { }

        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                this.OnPropertyChanged("Name");
            }
        }
        public Entity Type
        {
            get { return _Type; }
            set
            {
                _Type = value;
                this.OnPropertyChanged("Type");
            }
        }
        public object Value
        {
            get { return _Value; }
            set
            {
                _Value = value;
                this.OnPropertyChanged("Value");
            }
        }
    }
}
