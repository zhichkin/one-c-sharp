using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;

namespace Zhichkin.Hermes.UI
{
    public class TableField : BindableBase
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public bool IsUsed { get; set; }
    }

    public class BinaryExpression
    {
        public BinaryExpression(string type) { this.ExpressionType = type; }
        public string ExpressionType { get; set; }
    }
    public class ConditionalExpression : BinaryExpression
    {
        public ConditionalExpression(string type) : base(type) { }
        public string LeftExpression { get; set; }
        public string RightExpression { get; set; }
    }
    public class BinaryGroup : BinaryExpression
    {
        public BinaryGroup(string type) : base(type) { }
        public ObservableCollection<BinaryGroup> Children { get; set; }
        public ObservableCollection<ConditionalExpression> Conditions { get; set; }
    }

    public class TableDataSource : BindableBase
    {
        public TableDataSource() { }
        public string Name { get; set; }
        public string Alias { get; set; }
    }

    public class UserTable : TableDataSource
    {
        public UserTable() { }
        public ObservableCollection<TableField> Fields { get; set; }
    }
}
