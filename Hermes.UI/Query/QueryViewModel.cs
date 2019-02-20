using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Zhichkin.Shell;

namespace Zhichkin.Hermes.UI
{
    public class QueryViewModel : BindableBase
    {
        private readonly IUnityContainer container;

        public QueryViewModel(IUnityContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            this.container = container;
            this.InitializeTestData();
        }

        public string Name { get; set; }
        public ObservableCollection<TableField> Fields { get; set; }
        public ObservableCollection<BinaryExpression> WhereClause { get; set; }

        private void InitializeTestData()
        {
            this.Name = "TestTable";

            Fields = new ObservableCollection<TableField>()
            {
                new TableField() { Alias = "F1", Name = "Field_1", IsUsed = true },
                new TableField() { Alias = "F2", Name = "Field_2", IsUsed = false }
            };

            //WhereClause = new ObservableCollection<BinaryExpression>()
            //{
            //    new BinaryGroup("OR") { Children = new ObservableCollection<BinaryGroup>()
            //    {
            //        new BinaryGroup("AND") { Conditions = new ObservableCollection<ConditionalExpression>()
            //        {
            //            new ConditionalExpression("=") { LeftExpression = "Field_1", RightExpression = "1"},
            //            new ConditionalExpression("=") { LeftExpression = "Field_2", RightExpression = "2"}
            //        }},
            //        new BinaryGroup("AND") { Conditions = new ObservableCollection<ConditionalExpression>()
            //        {
            //            new ConditionalExpression("=") { LeftExpression = "Field_1", RightExpression = "3"},
            //            new ConditionalExpression("=") { LeftExpression = "Field_2", RightExpression = "4"}
            //        }}
            //    }}
            //};

            WhereClause = new ObservableCollection<BinaryExpression>()
            {
                new BinaryGroup("AND") { Conditions = new ObservableCollection<ConditionalExpression>()
                {
                    new ConditionalExpression("=") { LeftExpression = "Field_1", RightExpression = "1"},
                    new ConditionalExpression("=") { LeftExpression = "Field_2", RightExpression = "2"}
                }}
            };
        }
    }

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
    
}
