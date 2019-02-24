using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Unity;
using System;
using System.Collections.ObjectModel;

namespace Zhichkin.Hermes.UI
{
    public class SelectViewModel : TableExpression
    {
        private readonly IUnityContainer container;

        public SelectViewModel(IUnityContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            this.container = container;
            this.InitializeTestData();
        }
        public SelectViewModel() { this.InitializeTestData(); }
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
        public ObservableCollection<BinaryExpression> WhereClause { get; private set; }

        private void InitializeTestData()
        {
            this.Name = "TestSelect";
            this.Tables = new ObservableCollection<TableExpression>();

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
                    new ConditionalExpression("=") { LeftExpression = "Field_1" },
                    new ConditionalExpression("=") { LeftExpression = "Field_2" }
                }}
            };

            //Tables = new ObservableCollection<TableDataSource>()
            //{
            //    new UserTable() { Alias = "T1", Name = "Table_1" },
            //    new UserTable() { Alias = "T2", Name = "Table_2" }
            //};
        }
    }
}
