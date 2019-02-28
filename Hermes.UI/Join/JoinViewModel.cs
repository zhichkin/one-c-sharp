using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Zhichkin.Hermes.UI
{
    public class JoinViewModel : TableExpression
    {
        public JoinViewModel()
        {
            this.Filter = new FilterViewModel();
        }
        public string JoinType { get; set; }
        public TableExpression Table { get; set; }
        public FilterViewModel Filter { get; set; }
    }
}
