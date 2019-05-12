using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using Zhichkin.Hermes.Model;
using Zhichkin.Hermes.Services;
using Zhichkin.Shell;

namespace Zhichkin.Hermes.UI
{
    public class MainMenuViewModel : BindableBase
    {
        private const string CONST_ModuleDialogsTitle = "Hermes";

        private QueryExpressionViewModel queryVM;

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
            this.SaveQueryCommand = new DelegateCommand(this.SaveQuery);
            this.OpenQueryCommand = new DelegateCommand(this.OpenQuery);
            this.ChangeFromOrientationCommand = new DelegateCommand(this.ChangeFromOrientation);
            this.BuildMetadataTreeCommand = new DelegateCommand(this.BuildMetadataTree);
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
        public ICommand SaveQueryCommand { get; private set; }
        public ICommand OpenQueryCommand { get; private set; }
        public ICommand ChangeFromOrientationCommand { get; private set; }
        public ICommand BuildMetadataTreeCommand { get; private set; }
        
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

        private void AddNewQuery()
        {
            Z.ClearRightRegion(this.regionManager);
            IRegion rightRegion = this.regionManager.Regions[RegionNames.RightRegion];
            if (rightRegion == null) return;

            QueryExpression query = new QueryExpression(null);
            query.Expressions = new List<HermesModel>();
            queryVM = new QueryExpressionViewModel(null, query);

            SelectStatement model = new SelectStatement(query, null);
            query.Expressions.Add(model);
            SelectStatementViewModel select = new SelectStatementViewModel(queryVM, model);
            queryVM.QueryExpressions.Add(select);

            QueryExpressionView queryView = new QueryExpressionView(queryVM);
            
            rightRegion.Add(queryView);
        }
        private void ChangeFromOrientation()
        {
            if (queryVM.QueryExpressions.Count == 0) return;
            SelectStatementViewModel select = queryVM.QueryExpressions[0];
            select.IsFromVertical = !select.IsFromVertical;
        }

        private void SaveQuery()
        {
            QueryExpression model = queryVM.Model as QueryExpression;

            SerializationService serializer = new SerializationService();
            string json = serializer.ToJson(model);
            
            using (StreamWriter writer = new StreamWriter(GetQueryFilePath(), false))
            {
                writer.Write(json);
            }

            Z.Notify(new Notification() { Title = CONST_ModuleDialogsTitle, Content = "The query has been saved." });
        }
        private void OpenQuery()
        {
            string json = string.Empty;
            string path = GetQueryFilePath();
            using (StreamReader reader = new StreamReader(path))
            {
                json = reader.ReadToEnd();
            }

            QueryExpression query = null;
            try
            {
                SerializationService serializer = new SerializationService();
                query = serializer.FromJson(json);
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification() { Title = CONST_ModuleDialogsTitle, Content = Z.GetErrorText(ex) });
            }

            var psi = new ProcessStartInfo(@"notepad.exe")
            {
                Arguments = $"\"{path}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process.Start(psi);
            
            //Z.ClearRightRegion(this.regionManager);
            //IRegion rightRegion = this.regionManager.Regions[RegionNames.RightRegion];
            //if (rightRegion == null) return;
            //queryVM = new QueryExpressionViewModel(null, query);
            //QueryExpressionView queryView = new QueryExpressionView(queryVM);
            //rightRegion.Add(queryView);
        }
        private string GetQueryFilePath()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(assemblyPath);
            string catalogPath = Path.GetDirectoryName(uri.Path);
            return Path.Combine(catalogPath, "query.json");
        }

        private void BuildMetadataTree()
        {
            Z.ClearRightRegion(this.regionManager);
            IRegion rightRegion = this.regionManager.Regions[RegionNames.RightRegion];
            if (rightRegion == null) return;

            MetadataTreeViewModel model = new MetadataTreeViewModel();
            MetadataTreeView view = new MetadataTreeView();
            view.DataContext = model;
            rightRegion.Add(view);
        }
    }
}
