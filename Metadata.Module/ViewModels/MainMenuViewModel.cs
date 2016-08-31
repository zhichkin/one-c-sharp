using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Zhichkin.Metadata.Controllers;
using Zhichkin.Metadata.Notifications;
using Zhichkin.Metadata.Views;
using Zhichkin.Shell;

using Microsoft.Practices.Prism.Regions;

using Zhichkin.Metadata.Commands;

namespace Zhichkin.Metadata.ViewModels
{
    public class MainMenuViewModel : BindableBase
    {
        private readonly IRegionManager regionManager;
        private readonly IEventAggregator eventAggregator;

        public MainMenuViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");

            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;

            this.SQLConnectionPopupRequest = new InteractionRequest<SQLConnectionDialogNotification>();

            OpenMetadataCommand = new OpenMetadataCommand<object>(this.OnOpenMetadata, this.CanExecuteCommand);
            SaveMetadataCommand = new SaveMetadataCommand<object>(this.OnSaveMetadata, this.CanExecuteCommand);
            KillMetadataCommand = new KillMetadataCommand<object>(this.OnKillMetadata, this.CanExecuteCommand);
            ImportSQLMetadataCommand = new DelegateCommand(this.OpenSQLConnectionPopup);
            UpdateMetadataCommand = new UpdateMetadataCommand<object>(this.OnUpdateMetadata, this.CanExecuteCommand);
            ShowSettingsCommand = new ShowSettingsCommand<object>(this.OnShowSettings, this.CanExecuteCommand);
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

        public ICommand OpenMetadataCommand { get; private set; }
        public ICommand SaveMetadataCommand { get; private set; }
        public ICommand KillMetadataCommand { get; private set; }
        public ICommand ImportSQLMetadataCommand { get; private set; }
        public ICommand UpdateMetadataCommand { get; private set; }
        public ICommand ShowSettingsCommand { get; private set; }

        public InteractionRequest<SQLConnectionDialogNotification> SQLConnectionPopupRequest { get; private set; }

        private bool CanExecuteCommand(object args) { return true; }

        private void OnOpenMetadata(object args)
        {
            this.eventAggregator.GetEvent<OpenMetadataClicked>().Publish(args);
        }
        private void OnSaveMetadata(object args)
        {
            this.eventAggregator.GetEvent<MainMenuCommandClicked>().Publish(args);
        }
        private void OnKillMetadata(object args)
        {
            this.eventAggregator.GetEvent<MainMenuCommandClicked>().Publish(args);
        }
        private void OnUpdateMetadata(object args)
        {
            this.eventAggregator.GetEvent<MainMenuCommandClicked>().Publish(args);
        }
        private void OnShowSettings(object args)
        {
            this.eventAggregator.GetEvent<MainMenuCommandClicked>().Publish(args);
            MessageBox.Show("Settings");
        }
        private void OpenSQLConnectionPopup()
        {
            SQLConnectionDialogNotification notification = new SQLConnectionDialogNotification()
            {
                Title = "Импортировать метаданные SQL Server",
                Server = "server",
                Database = "database",
                UserName = "",
                Password = ""
            };
            if (this.MetadataTreeViewModel.CurrentInfoBase != null)
            {
                notification.Server   = MetadataTreeViewModel.CurrentInfoBase.Server;
                notification.Database = MetadataTreeViewModel.CurrentInfoBase.Database;
            }
            this.SQLConnectionPopupRequest.Raise(notification, response => { this.ImportSQLMetadata(response); });
        }
        private void ImportSQLMetadata(SQLConnectionDialogNotification notification)
        {
            if (!notification.Confirmed) return;
            this.eventAggregator.GetEvent<ImportSQLMetadataClicked>().Publish(notification);
        }
    }
}
