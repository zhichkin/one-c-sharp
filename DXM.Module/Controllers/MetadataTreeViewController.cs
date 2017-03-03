using System;
using System.Collections.Generic;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Zhichkin.Metadata.Services;
using Zhichkin.Metadata.Model;
using Zhichkin.Shell;

namespace Zhichkin.DXM.Module
{
    public class MetadataTreeViewController
    {
        private const string CONST_ModuleDialogsTitle = "Z-DXM";

        private readonly IUnityContainer container;
        private readonly IRegionManager regionManager;
        private readonly IEventAggregator eventAggregator;
        private readonly IMetadataService dataService;

        private readonly Dictionary<Type, Type> viewsLookup = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, Type> modelsLookup = new Dictionary<Type, Type>();

        public MetadataTreeViewController(IUnityContainer container,
                                  IRegionManager regionManager,
                                  IEventAggregator eventAggregator,
                                  IMetadataService dataService)
        {
            if (container == null) throw new ArgumentNullException("container");
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            if (dataService == null) throw new ArgumentNullException("dataService");
            this.container = container;
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            this.dataService = dataService;

            this.eventAggregator.GetEvent<MetadataInfoBaseSaveClicked>().Subscribe(this.InfoBaseSaveClicked, true);
            this.eventAggregator.GetEvent<MetadataInfoBaseKillClicked>().Subscribe(this.InfoBaseKillClicked, true);
            this.eventAggregator.GetEvent<MetadataTreeViewItemSelected>().Subscribe(this.TreeViewItemSelected, true);

            viewsLookup.Add(typeof(InfoBase), typeof(InfoBaseView));
            modelsLookup.Add(typeof(InfoBase), typeof(InfoBaseViewModel));
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
        private object GetView(object item)
        {
            Type itemType = item.GetType();
            Type viewType = null;
            viewsLookup.TryGetValue(itemType, out viewType);
            if (viewType == null) return null;
            Type modelType = modelsLookup[itemType];
            return this.container.Resolve(viewType, new ParameterOverride("model", item).OnType(modelType));
        }
        private void InfoBaseSaveClicked(InfoBase infoBase)
        {
            if (infoBase == null) throw new ArgumentNullException("infoBase");
            // process saving InfoBase node ...
        }
        private void InfoBaseKillClicked(InfoBase infoBase)
        {
            if (infoBase == null) throw new ArgumentNullException("infoBase");
            // process killing InfoBase node ...
        }
        private void TreeViewItemSelected(object item)
        {
            try
            {
                ClearRightRegion();
                if (item == null) return;
                IRegion rightRegion = this.regionManager.Regions[RegionNames.RightRegion];
                if (rightRegion == null) return;
                object view = GetView(item);
                if (view == null) return;
                rightRegion.Add(view);
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = ExceptionsHandling.GetErrorText(ex) });
            }
        }
    }
}