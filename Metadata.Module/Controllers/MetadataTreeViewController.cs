using System;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.Services;
using Zhichkin.Metadata.ViewModels;
using Zhichkin.Metadata.Views;
using Zhichkin.Shell;

namespace Zhichkin.Metadata.Controllers
{
    public class MetadataTreeViewController
    {
        private readonly IUnityContainer container;
        private readonly IRegionManager regionManager;
        private readonly IEventAggregator eventAggregator;
        private readonly IMetadataService dataService;

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

            this.eventAggregator.GetEvent<MetadataTreeViewItemSelected>().Subscribe(this.MetadataObjectSelected, true);
        }

        private void MetadataObjectSelected(object item)
        {
            IRegion rightRegion = this.regionManager.Regions[RegionNames.RightRegion];
            if (rightRegion == null) return;
            MetadataObjectView view = rightRegion.GetView("MetadataObjectView") as MetadataObjectView;

            if (view != null && item == null)
            {
                rightRegion.Remove(view);
                return;
            }

            if (view == null)
            {
                view = this.container.Resolve<MetadataObjectView>();
                rightRegion.Add(view, "MetadataObjectView");
            }
            else
            {
                rightRegion.Activate(view);
            }

            view.TextInfo.Text = item.ToString();
        }
    }
}