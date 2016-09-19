using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Zhichkin.Shell;
using Zhichkin.Metadata.SharedEvents;
using Zhichkin.Metadata.Model;
using Zhichkin.Integrator.Views;

namespace Zhichkin.Integrator.ViewModels
{
    public class IntegratorMainMenuViewModel : BindableBase
    {
        private const string CONST_ModuleDialogsTitle = "Z-Integrator";

        private readonly Dictionary<Type, Type> viewsLookup = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, Type> modelsLookup = new Dictionary<Type, Type>();

        private readonly IUnityContainer container;
        private readonly IRegionManager regionManager;
        private readonly IEventAggregator eventAggregator;

        public IntegratorMainMenuViewModel(IUnityContainer container, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            if (container == null) throw new ArgumentNullException("container");
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            this.container = container;
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            SetupViewsLookup();
            SetupModelsLookup();
            this.NotificationRequest = new InteractionRequest<INotification>();
            this.ConfirmationRequest = new InteractionRequest<IConfirmation>();
            this.ShowIntegratorSettingsCommand = new DelegateCommand(this.OnShowIntegratorSettings);
            this.eventAggregator.GetEvent<MetadataTreeViewItemSelected>().Subscribe(this.OnMetadataTreeViewItemSelected, true);
        }
        public InteractionRequest<INotification> NotificationRequest { get; private set; }
        public InteractionRequest<IConfirmation> ConfirmationRequest { get; private set; }
        public ICommand ShowIntegratorSettingsCommand { get; private set; }
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
        private void SetupViewsLookup()
        {
            viewsLookup.Add(typeof(InfoBase), typeof(InfoBaseView));
            viewsLookup.Add(typeof(Entity), typeof(PublisherView));
        }
        private void SetupModelsLookup()
        {
            modelsLookup.Add(typeof(InfoBase), typeof(InfoBaseViewModel));
            modelsLookup.Add(typeof(Entity), typeof(PublisherViewModel));
        }
        private object GetView(object item)
        {
            Type itemType = item.GetType();
            Type viewType = null;
            viewsLookup.TryGetValue(itemType, out viewType);
            if (viewType == null) return null;
            Type modelType = modelsLookup[itemType];
            return this.container.Resolve(viewType, new ParameterOverride("data", item).OnType(modelType));
        }
        private void ClearRightRegion()
        {
            IRegion rightRegion = this.regionManager.Regions[RegionNames.RightRegion];
            if (rightRegion == null) return;
            foreach (object view in rightRegion.Views)
            {
                rightRegion.Remove(view);
            }
        }
        private void OnMetadataTreeViewItemSelected(object item)
        {
            try
            {
                ShowMetadataItemView(item);
            }
            catch (Exception ex)
            {
                NotificationRequest.Raise(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            }
        }
        private void ShowMetadataItemView(object item)
        {
            ClearRightRegion();
            if (item == null) return;
            IRegion rightRegion = this.regionManager.Regions[RegionNames.RightRegion];
            if (rightRegion == null) return;
            object view = GetView(item);
            if (view == null) return;
            rightRegion.Add(view);
        }
        private void OnShowIntegratorSettings()
        {
            try
            {
                ShowIntegratorSettings();
            }
            catch (Exception ex)
            {
                NotificationRequest.Raise(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            }
        }
        private void ShowIntegratorSettings()
        {
            ClearRightRegion();
            IRegion rightRegion = this.regionManager.Regions[RegionNames.RightRegion];
            if (rightRegion == null) return;
            object view = this.container.Resolve(typeof(IntegratorSettingsView));
            if (view == null) return;
            rightRegion.Add(view);
        }
    }
}
