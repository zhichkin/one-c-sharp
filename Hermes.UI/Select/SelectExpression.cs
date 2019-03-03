using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Unity;
using System;
using System.Collections.ObjectModel;

namespace Zhichkin.Hermes.UI
{
    public class SelectExpression : TableExpression
    {
        public SelectExpression(TableExpression owner) : base(owner)
        {
            this.Tables = new ObservableCollection<TableExpression>();
            this.Filter = new BooleanExpression(this) { FilterType = "AND" };
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
        public ObservableCollection<TableExpression> Tables { get; set; }
        public BooleanExpression Filter { get; set; }
    }
}
