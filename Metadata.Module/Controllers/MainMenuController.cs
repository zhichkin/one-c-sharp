using System;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.Services;
using Zhichkin.Metadata.Notifications;
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

            this.eventAggregator.GetEvent<MainMenuSaveClicked>().Subscribe(this.MainMenuSaveClicked, true);
            this.eventAggregator.GetEvent<MainMenuKillClicked>().Subscribe(this.MainMenuKillClicked, true);
        }
        private MetadataTreeViewModel MetadataTreeViewModel
        {
            get
            {
                IRegion leftRegion = this.regionManager.Regions[RegionNames.LeftRegion];
                if (leftRegion == null) return null;
                MetadataTreeView view = leftRegion.Views.FirstOrDefault() as MetadataTreeView;
                if (view == null) return null;
                return view.DataContext as MetadataTreeViewModel; ;
            }
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
        private void MainMenuSaveClicked(MetadataTreeViewModel viewModel)
        {
            if (viewModel == null) throw new ArgumentNullException("viewModel");
            if (viewModel.CurrentInfoBase == null) throw new ArgumentNullException("viewModel.CurrentInfoBase");
            dataService.Save(viewModel.CurrentInfoBase);
            viewModel.CurrentInfoBase.OnPropertyChanged("State");
        }
        private void MainMenuKillClicked(MetadataTreeViewModel viewModel)
        {
            if (viewModel == null) throw new ArgumentNullException("viewModel");
            if (viewModel.CurrentInfoBase == null) throw new ArgumentNullException("viewModel.CurrentInfoBase");

            dataService.Kill(viewModel.CurrentInfoBase);
            viewModel.InfoBases.Remove(viewModel.CurrentInfoBase);
            ClearRightRegion();
        }
    }
}