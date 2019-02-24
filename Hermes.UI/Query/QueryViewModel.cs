using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Zhichkin.Hermes.UI
{
    public class QueryViewModel : BindableBase
    {
        public QueryViewModel()
        {
            this.QueryParameters = new ObservableCollection<ParameterViewModel>();
            this.QueryExpressions = new ObservableCollection<SelectViewModel>();
        }
        public ObservableCollection<ParameterViewModel> QueryParameters { get; private set; }
        public ObservableCollection<SelectViewModel> QueryExpressions { get; private set; }
    }
}
