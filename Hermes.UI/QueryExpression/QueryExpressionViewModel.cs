using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Zhichkin.Hermes.UI
{
    public class QueryExpressionViewModel : HermesViewModel
    {
        public QueryExpressionViewModel(HermesViewModel parent) : base(parent)
        {
            this.QueryParameters = new ObservableCollection<ParameterExpressionViewModel>();
            this.QueryExpressions = new ObservableCollection<SelectStatementViewModel>();
        }
        public ObservableCollection<ParameterExpressionViewModel> QueryParameters { get; private set; }
        public ObservableCollection<SelectStatementViewModel> QueryExpressions { get; private set; }
    }
}
