using System;
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
    public class MainMenuController
    {
        private readonly IUnityContainer container;
        private readonly IRegionManager regionManager;
        private readonly IEventAggregator eventAggregator;
        private readonly IMetadataService dataService;

        public MainMenuController(IUnityContainer container,
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

        /// <summary>
        /// Called when a new employee is selected. This method uses
        /// view injection to programmatically 
        /// </summary>
        private void MetadataObjectSelected(object item)
        {
            //this.dataService.GetInfoBases(item);

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

            view.TextInfo.Text = (string)item;

            // Set the current employee property on the view model.
            //EmployeeSummaryViewModel viewModel = view.DataContext as EmployeeSummaryViewModel;
            //if (viewModel != null)
            //{
            //    viewModel.CurrentEmployee = selectedEmployee;
            //}
        }
    }
}