using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Unity;
using System;
using System.Collections.ObjectModel;

namespace Zhichkin.Hermes.UI
{
    public class SelectViewModel : TableExpression
    {
        public SelectViewModel()
        {
            this.Fields = new ObservableCollection<TableField>();
            this.Tables = new ObservableCollection<TableExpression>();
            this.Filter = new FilterViewModel();
        }
        private bool _IsFromVertical = true;
        private string _FromClauseDescription = "Tabular data source names ...";
        public bool IsFromVertical
        {
            get { return _IsFromVertical; }
            set
            {
                _IsFromVertical = value;
                this.OnPropertyChanged("IsFromVertical");
            }
        }
        public string FromClauseDescription
        {
            get { return _FromClauseDescription; }
            private set
            {
                _FromClauseDescription = value;
                this.OnPropertyChanged("FromClauseDescription");
            }
        }
        public ObservableCollection<TableField> Fields { get; private set; }
        public ObservableCollection<TableExpression> Tables { get; private set; }
        public FilterViewModel Filter { get; private set; }
    }
}
