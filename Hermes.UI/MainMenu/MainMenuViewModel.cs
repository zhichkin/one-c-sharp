using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using System;
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
            this.AddNewQueryCommand = new DelegateCommand(this.OnAddNewQueryCommand);
        }
        public ICommand AddNewQueryCommand { get; private set; }
        private void OnAddNewQueryCommand()
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
            //Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = "Hello from Hermes!" });
            Z.ClearRightRegion(this.regionManager);
            IRegion rightRegion = this.regionManager.Regions[RegionNames.RightRegion];
            if (rightRegion == null) return;
            object view = this.container.Resolve(typeof(QueryView));
            if (view == null) return;
            rightRegion.Add(view);
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
    }
}
