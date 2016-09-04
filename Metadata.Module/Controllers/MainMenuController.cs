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
using System.Data.SqlClient;

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

            this.eventAggregator.GetEvent<OpenMetadataClicked>().Subscribe(this.OpenMetadataClicked, true);
            this.eventAggregator.GetEvent<MainMenuSaveClicked>().Subscribe(this.MainMenuSaveClicked, true);
            this.eventAggregator.GetEvent<MainMenuKillClicked>().Subscribe(this.MainMenuKillClicked, true);
            this.eventAggregator.GetEvent<ImportSQLMetadataClicked>().Subscribe(this.ImportSQLMetadataClicked, true);
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

        private void OpenMetadataClicked(object item)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Файлы XML(*.xml)|*.xml" + "|Все файлы (*.*)|*.* ";
            dialog.CheckFileExists = true;
            dialog.Multiselect = false;
            if (dialog.ShowDialog() != true) return;

            IRegion leftRegion = this.regionManager.Regions[RegionNames.LeftRegion];
            if (leftRegion == null) return;
            MetadataTreeView view = leftRegion.Views.FirstOrDefault() as MetadataTreeView;
            if (view == null) return;
            MetadataTreeViewModel model = view.DataContext as MetadataTreeViewModel;
            if (model == null) return;

            try
            {
                InfoBase infoBase = new InfoBase();
                (new XMLMetadataAdapter()).Load(dialog.FileName, infoBase);
                model.InfoBases.Add(infoBase);
            }
            catch (Exception ex)
            {
                MessageBoxResult result = MessageBox.Show(ex.Message);
            }
        }

        private void ImportSQLMetadataClicked(object info)
        {
            if (info == null) throw new ArgumentNullException("info");
            SQLConnectionDialogNotification notification = info as SQLConnectionDialogNotification;
            if (notification == null) throw new ArgumentNullException("notification");
            if(this.MetadataTreeViewModel == null) throw new ArgumentNullException("MetadataTreeViewModel");

            InfoBase infoBase = this.MetadataTreeViewModel.CurrentInfoBase;
            if (infoBase == null) return;

            SqlConnectionStringBuilder helper = new SqlConnectionStringBuilder()
            {
                DataSource = notification.Server,
                InitialCatalog = notification.Database,
                IntegratedSecurity = string.IsNullOrWhiteSpace(notification.UserName)
            };
            if (!helper.IntegratedSecurity)
            {
                helper.UserID = notification.UserName;
                helper.Password = notification.Password;
            }

            (new SQLMetadataAdapter()).Load(helper.ToString(), infoBase);
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
        }
    }
}