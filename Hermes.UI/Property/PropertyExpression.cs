using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows;

namespace Zhichkin.Hermes.UI
{
    public class PropertyExpression : FunctionExpression
    {
        public PropertyExpression(TableExpression owner) : base(owner) { }
        public string Alias { get; set; }
        public FunctionExpression Function { get; set; }
        public string FullName { get { return string.Format("{0}.{1}", this.Owner.Alias, this.Name); } }

        private TableExpression _Table;
        public TableExpression Table
        {
            get { return _Table; }
            set
            {
                _Table = value;
                OnPropertyChanged("Table");
            }
        }
        private PropertyExpression _Field;
        public PropertyExpression Field
        {
            get { return _Field; }
            set
            {
                _Field = value;
                OnPropertyChanged("Field");
            }
        }
    }
}
