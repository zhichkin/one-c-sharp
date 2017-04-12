using Microsoft.Practices.Prism.Mvvm;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Metadata.UI
{
    public sealed class ChameleonBoxViewModel : BindableBase
    {
        private InfoBase _InfoBase;
        private Entity _ValueType;
        private object _Value;

        public ChameleonBoxViewModel() { }

        public InfoBase InfoBase
        {
            get { return _InfoBase; }
            set { _InfoBase = value; OnPropertyChanged("InfoBase"); }
        }
        public Entity ValueType
        {
            get { return _ValueType; }
            set { _ValueType = value; OnPropertyChanged("ValueType"); }
        }
        public object Value
        {
            get { return _Value; }
            set { _Value = value; OnPropertyChanged("Value"); }
        }
    }
}
