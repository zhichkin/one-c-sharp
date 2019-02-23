using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.UI
{
    public class ParameterViewModel : BindableBase
    {
        private string _Name = "ParameterName";
        private Entity _Type = null;
        private object _Value = null;

        public ParameterViewModel() { }
        
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
        public ObservableCollection<TableField> Fields { get; set; }
    }
}
