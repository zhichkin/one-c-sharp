using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows.Input;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class ComparisonOperatorViewModel : BindableBase
    {
        public ComparisonOperatorViewModel(ComparisonOperator model)
        {
            this.Model = model;
            this.AddNewOperatorCommand = new DelegateCommand(this.AddNewOperator);
        }
        public ComparisonOperator Model { get; private set; }
        public string Name
        {
            get { return this.Model.Name; }
            set
            {
                this.Model.Name = value;
                this.OnPropertyChanged("Name");
            }
        }
        public ICommand AddNewOperatorCommand { get; private set; }
        public void AddNewOperator()
        {
            this.Name = "add clicked";
        }
    }
}
