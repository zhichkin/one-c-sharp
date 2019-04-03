using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.UI
{
    public class PropertyExpression : BindableBase
    {
        private string _Alias;
        private Property _Property;

        public PropertyExpression() { }
        
        public string Alias
        {
            get { return _Alias; }
            set
            {
                _Alias = value;
                OnPropertyChanged("Alias");
            }
        }
        public Property Property
        {
            get { return _Property; }
            set
            {
                _Property = value;
                OnPropertyChanged("Property");
            }
        }
    }
}
