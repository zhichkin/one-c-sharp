using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Zhichkin.Hermes.UI
{
    public class QueryExpression : BindableBase
    {
        public QueryExpression()
        {
            this.QueryParameters = new ObservableCollection<ParameterExpressionViewModel>();
            this.QueryExpressions = new ObservableCollection<SelectStatementViewModel>();
        }
        public ObservableCollection<ParameterExpressionViewModel> QueryParameters { get; private set; }
        public ObservableCollection<SelectStatementViewModel> QueryExpressions { get; private set; }
    }
}
