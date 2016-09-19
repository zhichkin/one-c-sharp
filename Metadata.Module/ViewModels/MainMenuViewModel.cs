using System;
using System.Linq;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Zhichkin.Metadata.Controllers;
using Zhichkin.Metadata.Notifications;
using Zhichkin.Metadata.Views;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.Services;
using Zhichkin.Shell;
using System.Data.SqlClient;
using Microsoft.Practices.Prism.Regions;
using Zhichkin.Metadata.Commands;

namespace Zhichkin.Metadata.ViewModels
{
    public class MainMenuViewModel : BindableBase
    {
        private const string CONST_ModuleDialogsTitle = "Z-Metadata";
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

            this.NotificationRequest = new InteractionRequest<INotification>();
            this.ConfirmationRequest = new InteractionRequest<IConfirmation>();
            this.SQLConnectionPopupRequest = new InteractionRequest<SQLConnectionDialogNotification>();

            OpenMetadataCommand = new OpenMetadataCommand<object>(this.OnOpenMetadata, this.CanExecuteCommand);
            SaveMetadataCommand = new DelegateCommand(this.OnSaveMetadata);
            KillMetadataCommand = new DelegateCommand(this.OnKillMetadata);
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
        public ICommand UpdateMetadataCommand { get; private set; }
        public ICommand ShowSettingsCommand { get; private set; }

        public InteractionRequest<INotification> NotificationRequest { get; private set; }
        public InteractionRequest<IConfirmation> ConfirmationRequest { get; private set; }
        public InteractionRequest<SQLConnectionDialogNotification> SQLConnectionPopupRequest { get; private set; }

        private bool CanExecuteCommand(object args) { return true; }
        private void ClearRightRegion()
        {
            IRegion rightRegion = this.regionManager.Regions[RegionNames.RightRegion];
            if (rightRegion == null) return;
            foreach (object view in rightRegion.Views)
            {
                rightRegion.Remove(view);
            }
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

        private void OnSaveMetadata()
        {
            if (this.MetadataTreeViewModel.CurrentInfoBase == null) return;
            try
            {
                this.eventAggregator.GetEvent<MainMenuSaveClicked>().Publish(this.MetadataTreeViewModel);
                this.NotificationRequest.Raise(new Notification
                    {
                        Title = CONST_ModuleDialogsTitle,
                        Content = string.Format(
                            "Метаданные информационной базы \"{0}\" сохранены.",
                            this.MetadataTreeViewModel.CurrentInfoBase.ToString())
                    });
            }
            catch (Exception ex)
            {
                NotificationRequest.Raise(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            }
        }
        private void OnKillMetadata()
        {
            if (this.MetadataTreeViewModel.CurrentInfoBase == null) return;
            this.ConfirmationRequest.Raise(new Confirmation
                {
                    Title = CONST_ModuleDialogsTitle,
                    Content = string.Format(
                        "Информация об информационной базе \"{0}\"\nбудет полностью удалена. Продолжить?",
                        this.MetadataTreeViewModel.CurrentInfoBase.ToString())
                },
                c => { if (c.Confirmed) this.KillMetadata(); });
        }
        private void KillMetadata()
        {
            try
            {
                this.eventAggregator.GetEvent<MainMenuKillClicked>().Publish(this.MetadataTreeViewModel);
            }
            catch (Exception ex)
            {
                NotificationRequest.Raise(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            }
        }
        private void OnUpdateMetadata(object args)
        {
            this.NotificationRequest.Raise(new Notification
               {
                   Title = CONST_ModuleDialogsTitle,
                   Content = "Мы работаем над этим ..."
               });
        }
        private void OnShowSettings(object args)
        {
            try
            {
                ShowSettings();
            }
            catch (Exception ex)
            {
                NotificationRequest.Raise(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            }
        }
        private void ShowSettings()
        {
            ClearRightRegion();
            IRegion rightRegion = this.regionManager.Regions[RegionNames.RightRegion];
            if (rightRegion == null) return;
            object view = this.container.Resolve(typeof(MetadataSettingsView));
            if (view == null) return;
            rightRegion.Add(view);
        }
        private void OnOpenMetadata(object args)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Файлы XML(*.xml)|*.xml" + "|Все файлы (*.*)|*.* ";
            dialog.CheckFileExists = true;
            dialog.Multiselect = false;
            if (dialog.ShowDialog() != true) return;
            if (MetadataTreeViewModel == null) return;
            try
            {
                InfoBase infoBase = new InfoBase();
                (new XMLMetadataAdapter()).Load(dialog.FileName, infoBase);
                bool cancel = OpenSQLConnectionPopup(infoBase);
                if (cancel)
                {
                    NotificationRequest.Raise(new Notification { Title = CONST_ModuleDialogsTitle, Content = "Действие отменено пользователем." });
                }
                else
                {
                    MetadataTreeViewModel.InfoBases.Add(infoBase);
                }
            }
            catch (Exception ex)
            {
                NotificationRequest.Raise(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            }
        }
        private bool OpenSQLConnectionPopup(InfoBase infoBase)
        {
            bool cancel = false;
            SQLConnectionDialogNotification notification = new SQLConnectionDialogNotification()
            {
                Title = CONST_ModuleDialogsTitle,
                Server = infoBase.Server,
                Database = infoBase.Database,
                UserName = "",
                Password = ""
            };
            this.SQLConnectionPopupRequest.Raise(notification, response =>
            {
                if (!response.Confirmed)
                {
                    cancel = true;
                }
                else
                {
                    this.ImportSQLMetadata(infoBase, response);
                }
            });
            return cancel;
        }
        private void ImportSQLMetadata(InfoBase infoBase, SQLConnectionDialogNotification response)
        {
            SqlConnectionStringBuilder helper = new SqlConnectionStringBuilder()
            {
                DataSource = response.Server,
                InitialCatalog = response.Database,
                IntegratedSecurity = string.IsNullOrWhiteSpace(response.UserName)
            };
            if (!helper.IntegratedSecurity)
            {
                helper.UserID = response.UserName;
                helper.Password = response.Password;
            }
            infoBase.Server = helper.DataSource;
            infoBase.Database = helper.InitialCatalog;
            (new SQLMetadataAdapter()).Load(helper.ToString(), infoBase);
        }
    }
}
