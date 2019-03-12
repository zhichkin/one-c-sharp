using Microsoft.Practices.Prism.Mvvm;
using System.Collections.Generic;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Metadata.UI
{
    public sealed class FilterParameterViewModel : BindableBase
    {
        private static readonly List<string> _operators = new List<string>()
        {
            "Равно",
            "Не равно",
            "Содержит",
            "Больше",
            "Больше или равно",
            "Меньше",
            "Меньше или равно",
            "Между"
        };

        private object _value;
        
        public bool UseMe { get; set; }
        public InfoBase InfoBase { set; get; }
        public string Name { set; get; }
        public Entity Type { set; get; }
        public FilterOperator FilterOperator { set; get; }
        public object Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (_value == null)
                {
                    this.Type = Entity.Empty;
                }
                else
                {
                    Entity metadata = Entity.GetMetadataType(_value.GetType());
                    if (metadata == Entity.Object)
                    {
                        metadata = ((ReferenceProxy)_value).Type; // !!! ReferenceProxy !!!
                    }
                    if (metadata != this.Type)
                    {
                        this.Type = metadata;
                    }
                }
                this.OnPropertyChanged("Value");
            }
        }

        public List<string> FilterOperators { get { return _operators; } }
        private string _selectedFilterOperator = "Равно";
        public string SelectedFilterOperator
        {
            get { return _selectedFilterOperator; }
            set
            {
                _selectedFilterOperator = value;
                if (_selectedFilterOperator == "Равно") this.FilterOperator = FilterOperator.Equal;
                else if (_selectedFilterOperator == "Не равно") this.FilterOperator = FilterOperator.NotEqual;
                else if (_selectedFilterOperator == "Содержит") this.FilterOperator = FilterOperator.Contains;
                else if (_selectedFilterOperator == "Больше") this.FilterOperator = FilterOperator.Greater;
                else if (_selectedFilterOperator == "Больше или равно") this.FilterOperator = FilterOperator.GreaterOrEqual;
                else if (_selectedFilterOperator == "Меньше") this.FilterOperator = FilterOperator.Less;
                else if (_selectedFilterOperator == "Меньше или равно") this.FilterOperator = FilterOperator.LessOrEqual;
                else if (_selectedFilterOperator == "Между") this.FilterOperator = FilterOperator.Between;
            }
        }
    }
}
