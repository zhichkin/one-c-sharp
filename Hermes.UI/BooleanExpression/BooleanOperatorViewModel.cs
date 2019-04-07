using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class BooleanOperatorViewModel : BindableBase
    {
        public BooleanOperatorViewModel(BooleanOperator model)
        {
            this.Model = model;
            this.Operands = new ObservableCollection<BooleanFunction>(this.Model.Operands);
            this.Operands.CollectionChanged += Operands_CollectionChanged;
        }
        private void Operands_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object item in e.NewItems)
                {
                    this.Model.Operands.Add((BooleanFunction)item);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (object item in e.OldItems)
                {
                    this.Model.Operands.Remove((BooleanFunction)item);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.Model.Operands.Clear();
            }
        }
        public BooleanOperator Model { get; private set; }
        public string Name
        {
            get { return this.Model.Name; }
            set
            {
                this.Model.Name = value;
                this.OnPropertyChanged("Name");
            }
        }
        public ObservableCollection<BooleanFunction> Operands { get; } // ???
    }
}
