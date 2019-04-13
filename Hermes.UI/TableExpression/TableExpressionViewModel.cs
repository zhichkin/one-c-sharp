using Microsoft.Practices.Prism.Mvvm;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class TableExpressionViewModel : BindableBase
    {
        public TableExpressionViewModel(TableExpression model)
        {
            this.Model = model;
        }
        public TableExpression Model { get; set; }
        public TableExpressionViewModel Parent { get; set; }
        public string Name
        {
            get { return this.Model?.Name; }
        }
        public string Alias
        {
            get { return this.Model?.Alias; }
            set
            {
                if (this.Model == null) return; // TODO: what can I do with it ???
                this.Model.Alias = value;
                this.OnPropertyChanged("Alias");
            }
        }
    }
}
