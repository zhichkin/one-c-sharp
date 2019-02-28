using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Zhichkin.Hermes.UI
{
    public class TableExpression : BindableBase
    {
        public TableExpression() { }
        public string Name { get; set; }
        public string Alias { get; set; }
    }

    public class TableViewModel : TableExpression
    {
        public TableViewModel() { }
        public ObservableCollection<TableField> Fields { get; set; }
    }
}
