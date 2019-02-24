using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;
using Zhichkin.Hermes.Infrastructure;

namespace Zhichkin.Hermes.UI
{
    public class TypeInfo : ITypeInfo
    {
        public int Code { get; set; }
        public string Name { get; set; }
    }

    public class TableField : BindableBase
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public bool IsUsed { get; set; }
    }

    public class BinaryExpression : BindableBase
    {
        public BinaryExpression(string type) { this.ExpressionType = type; }
        public string ExpressionType { get; set; }
    }
    public class ConditionalExpression : BinaryExpression
    {
        public ConditionalExpression(string type) : base(type) { }
        public string LeftExpression { get; set; }
        ParameterViewModel _RightExpression;
        public ParameterViewModel RightExpression
        {
            get { return _RightExpression; }
            set
            {
                _RightExpression = value;
                this.OnPropertyChanged("RightExpression");
            }
        }
    }
    public class BinaryGroup : BinaryExpression
    {
        public BinaryGroup(string type) : base(type) { }
        public ObservableCollection<BinaryGroup> Children { get; set; }
        public ObservableCollection<ConditionalExpression> Conditions { get; set; }
    }

    public class TableExpression : BindableBase
    {
        public TableExpression() { }
        public string Name { get; set; }
        public string Alias { get; set; }
    }

    public class TableViewModel : TableExpression
    {
        public TableViewModel() { }
        public ObservableCollection<TableField> Fields { get; set; }
    }
}
