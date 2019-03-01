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
            this.QueryParameters = new ObservableCollection<ParameterExpression>();
            this.QueryExpressions = new ObservableCollection<SelectExpression>();
        }
        public ObservableCollection<ParameterExpression> QueryParameters { get; private set; }
        public ObservableCollection<SelectExpression> QueryExpressions { get; private set; }
    }
}
