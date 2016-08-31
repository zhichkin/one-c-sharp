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
            this.eventAggregator.GetEvent<ImportSQLMetadataClicked>().Subscribe(this.ImportSQLMetadataClicked, true);
            this.eventAggregator.GetEvent<MainMenuCommandClicked>().Subscribe(this.MainMenuCommandClicked, true);
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
                InfoBase infoBase = this.dataService.GetMetadata(dialog.FileName);
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

        private void MainMenuCommandClicked(object item)
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

            view.TextInfo.Text = (string)item;
        }
    }
}