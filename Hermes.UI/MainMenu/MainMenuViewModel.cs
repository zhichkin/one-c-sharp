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
    public class MainMenuViewModel : BindableBase
    {
        private const string CONST_ModuleDialogsTitle = "Hermes";

        private readonly IUnityContainer container;
        private readonly IRegionManager regionManager;
        private readonly IEventAggregator eventAggregator;

        public MainMenuViewModel(IUnityContainer container, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            if (container == null) throw new ArgumentNullException("container");
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            this.container = container;
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            this.AddNewQueryCommand = new DelegateCommand(this.AddNewQuery);
            this.AddNewSelectStatementCommand = new DelegateCommand(this.AddNewSelectStatement);
            this.AddTableToSelectStatementCommand = new DelegateCommand(this.AddTableToSelectStatement);
            this.ChangeFromOrientationCommand = new DelegateCommand(this.ChangeFromOrientation);
        }
        private string GetErrorText(Exception ex)
        {
            string errorText = string.Empty;
            Exception error = ex;
            while (error != null)
            {
                errorText += (errorText == string.Empty) ? error.Message : Environment.NewLine + error.Message;
                error = error.InnerException;
            }
            return errorText;
        }
        public ICommand AddNewQueryCommand { get; private set; }
        public ICommand AddNewSelectStatementCommand { get; private set; }
        public ICommand AddTableToSelectStatementCommand { get; private set; }
        public ICommand ChangeFromOrientationCommand { get; private set; }
        
        private void ErrorProneCommand()
        {
            try
            {
                AddNewQuery();
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            }
        }

        private QueryExpression query;
        private void AddNewQuery()
        {
            //Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = "Hello from Hermes!" });
            Z.ClearRightRegion(this.regionManager);
            IRegion rightRegion = this.regionManager.Regions[RegionNames.RightRegion];
            if (rightRegion == null) return;

            query = new QueryExpression();
            query.QueryParameters.Add(new ParameterExpression(query) { Name = "Parameter0", Type = new TypeInfo() { Code = 1, Name = "GUID" } });
            query.QueryParameters.Add(new ParameterExpression(query) { Name = "Parameter1", Type = new TypeInfo() { Code = 2, Name = "Int32" } });
            query.QueryParameters.Add(new ParameterExpression(query) { Name = "Parameter2", Type = new TypeInfo() { Code = 3, Name = "String" } });
            query.QueryParameters.Add(new ParameterExpression(query) { Name = "Parameter3", Type = new TypeInfo() { Code = 4, Name = "Boolean" } });

            SelectExpression select = new SelectExpression(null);
            InitializeTestData(select);
            query.QueryExpressions.Add(select);

            QueryView queryView = new QueryView(query);
            
            rightRegion.Add(queryView);
        }
        private void AddNewSelectStatement()
        {
            SelectExpression select = new SelectExpression(null);
            InitializeTestData(select);
            query.QueryExpressions.Add(select);
        }
        private void AddTableToSelectStatement()
        {
            if (query.QueryExpressions.Count == 0) return;
            SelectExpression select = query.QueryExpressions[0];
            select.Tables.Add(new EntityExpression(select) { Name = "Table-1", Alias = "T1" });

            JoinExpression join = new JoinExpression(select) { JoinType = "LEFT JOIN" };
            join.Table = new EntityExpression(select) { Name = "Table-2", Alias = "T2" };
            join.Filter.FilterType = "OR";

            BooleanExpression and_1 = new BooleanExpression(join) { FilterType = "AND" };
            and_1.Conditions.Add(new ComparisonExpression(join) { LeftExpression = "Field_1" });
            and_1.Conditions.Add(new ComparisonExpression(join) { LeftExpression = "Field_2" });
            join.Filter.Children.Add(and_1);

            BooleanExpression or_1 = new BooleanExpression(join) { FilterType = "OR" };
            BooleanExpression and_2 = new BooleanExpression(join) { FilterType = "AND" };
            and_2.Conditions.Add(new ComparisonExpression(join) { LeftExpression = "Field_3" });
            and_2.Conditions.Add(new ComparisonExpression(join) { LeftExpression = "Field_4" });
            BooleanExpression and_3 = new BooleanExpression(join) { FilterType = "AND" };
            and_3.Conditions.Add(new ComparisonExpression(join) { LeftExpression = "Field_5" });
            and_3.Conditions.Add(new ComparisonExpression(join) { LeftExpression = "Field_6" });
            or_1.Children.Add(and_2);
            or_1.Children.Add(and_3);
            join.Filter.Children.Add(or_1);

            select.Tables.Add(join);

            select.Tables.Add(new SelectExpression(select));
        }
        private void ChangeFromOrientation()
        {
            if (query.QueryExpressions.Count == 0) return;
            SelectExpression select = query.QueryExpressions[0];
            select.IsFromVertical = !select.IsFromVertical;
        }

        private void InitializeTestData(SelectExpression select)
        {
            select.Alias = "TestSelect";

            select.Fields = HermesUI.GetTestEntityFields(select);

            select.Filter.Conditions.Add(new ComparisonExpression(select) { LeftExpression = "Field_1" });
            select.Filter.Conditions.Add(new ComparisonExpression(select) { LeftExpression = "Field_2" });
        }
    }
}
