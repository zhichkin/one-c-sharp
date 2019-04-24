using System;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class PropertyReferenceViewModel: HermesViewModel
    {
        private TableExpressionViewModel _Table;
        public PropertyReferenceViewModel(HermesViewModel parent, TableExpressionViewModel table, PropertyReference model) : base(parent, model)
        {
            this.Table = table;
        }
        public TableExpressionViewModel Table
        {
            get { return _Table; }
            set
            {
                _Table = value;
                this.OnPropertyChanged("Table");
            }
        }
        public PropertyReferenceViewModel Property
        {
            get { return this; }
            set
            {
                // Свойство устанваливается при выборе значения связанного источника данных ComboBox'а
                if (this.Table != null && value != null && this.Table != value.Table)
                {
                    throw new Exception("Что-то пошло не так с выбором свойства =|");
                }
                this.Model = (value == null) ? null : value.Model;
            }
        }
        public string Name
        {
            get
            {
                if (this.Model == null) return string.Empty;
                return ((PropertyReference)this.Model).Name;
            }
        }
    }
}
