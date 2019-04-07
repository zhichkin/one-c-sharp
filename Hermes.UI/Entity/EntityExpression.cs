using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.UI
{
    public class TableExpression : BindableBase
    {
        private string _Alias;

        public TableExpression()
        {
            this.Fields = new ObservableCollection<PropertyExpressionViewModel>();
        }

        public string Alias
        {
            get { return _Alias; }
            set
            {
                _Alias = value;
                OnPropertyChanged("Alias");
            }
        }
        public virtual ObservableCollection<PropertyExpressionViewModel> Fields { get; set; }
    }

    public class EntityExpression : TableExpression
    {
        private Entity _Entity;

        public EntityExpression() { }

        public Entity Entity
        {
            get { return _Entity; }
            set
            {
                _Entity = value;
                OnPropertyChanged("Entity");
            }
        }
    }
}
