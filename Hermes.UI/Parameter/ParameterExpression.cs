using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;
using Zhichkin.Hermes.Infrastructure;

namespace Zhichkin.Hermes.UI
{
    public class ParameterExpression : BindableBase, IParameter
    {
        private readonly QueryExpression _Query;
        private string _Name = "ParameterName";
        private ITypeInfo _Type = null;
        private object _Value = null; // ? can be a default value

        public ParameterExpression(QueryExpression query) { _Query = query; }
        
        public QueryExpression Query { get { return _Query; } }
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                this.OnPropertyChanged("Name");
            }
        }
        public ITypeInfo Type
        {
            get { return _Type; }
            set
            {
                _Type = value;
                this.OnPropertyChanged("Type");
            }
        }
        public object Value
        {
            get { return _Value; }
            set
            {
                _Value = value;
                this.OnPropertyChanged("Value");
            }
        }
        //public ObservableCollection<TableField> Fields { get; set; } - class TableParameter : IParameter

        public override string ToString()
        {
            return string.Format("@{0} ({1})", this.Name, this.Type.Name);
        }
    }
}
