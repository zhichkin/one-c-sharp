using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Zhichkin.ChangeTracking;
using Zhichkin.Metadata.Model;
using Zhichkin.Integrator.Model;

namespace Zhichkin.Integrator.ViewModels
{
    public class PublisherViewModel : BindableBase
    {
        private Publisher publisher = null;
        private readonly Entity entity;
        private readonly InfoBase infoBase;
        private readonly IRegionManager regionManager;
        private readonly IEventAggregator eventAggregator;

        private ChangeTrackingDatabaseInfo _ChangeTrackingDatabaseInfo = null;
        private ChangeTrackingTableInfo _ChangeTrackingTableInfo = null;
        
        public PublisherViewModel(Entity data, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            entity = data;
            infoBase = entity.Namespace.InfoBase;
            publisher = Publisher.Select(entity.Identity);
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;

            this.NotificationRequest = new InteractionRequest<INotification>();

            this.UpdateTextBoxSourceCommand = new DelegateCommand<object>(this.OnUpdateTextBoxSource);
            this.PublishChangesCommand = new DelegateCommand(this.OnPublishChanges);
            this.ProcessMessagesCommand = new DelegateCommand(this.OnProcessMessages);
            InitializeViewModel();
        }
        public ICommand UpdateTextBoxSourceCommand { get; private set; }
        private void OnUpdateTextBoxSource(object userControl)
        {
            TextBox textBox = userControl as TextBox;
            if (textBox == null) return;
            DependencyProperty property = TextBox.TextProperty;
            BindingExpression binding = BindingOperations.GetBindingExpression(textBox, property);
            if (binding == null) return;
            binding.UpdateSource();
        }
        public InteractionRequest<INotification> NotificationRequest { get; private set; }

        public string Name
        {
            get { return (entity == null) ? string.Empty : entity.Name; }
        }
        public string MainTable
        {
            get { return (entity?.MainTable == null) ? string.Empty : entity.MainTable.Name; }
        }

        public void InitializeViewModel()
        {
            ChangeTrackingService services = new ChangeTrackingService(infoBase.ConnectionString);
            try
            {
                _ChangeTrackingDatabaseInfo = services.GetChangeTrackingDatabaseInfo(infoBase);
            }
            catch
            {
                _ChangeTrackingDatabaseInfo = null;
            }
            if (_ChangeTrackingDatabaseInfo == null) return;

            _ChangeTrackingTableInfo = services.GetChangeTrackingTableInfo(entity.MainTable);
        }
        public bool IsDatabaseChangeTrackingEnabled
        {
            get { return _ChangeTrackingDatabaseInfo != null; }
        }
        public bool IsChangeTrackingEnabled
        {
            get { return IsDatabaseChangeTrackingEnabled && _ChangeTrackingTableInfo != null; }
            set
            {
                try
                {
                    if (IsChangeTrackingEnabled)
                    {
                        DisableChangeTracking();
                    }
                    else
                    {
                        EnableChangeTracking();
                    }
                    InitializeViewModel();
                    OnPropertyChanged("IsChangeTrackingEnabled");
                    OnPropertyChanged("LastSyncVersion");
                    OnPropertyChanged("MSMQTargetQueue");
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }
        public bool IsColumnsTrackingEnabled
        {
            get { return _ChangeTrackingTableInfo != null && _ChangeTrackingTableInfo.IS_TRACK_COLUMNS_UPDATED_ON; }
            set
            {
                ChangeTrackingService services = new ChangeTrackingService(infoBase.ConnectionString);
                try
                {
                    services.SwitchTableColumnsTracking(entity.MainTable, value);
                    InitializeViewModel();
                    OnPropertyChanged("IsColumnsTrackingEnabled");
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }
        private void EnableChangeTracking()
        {
            ChangeTrackingService service = new ChangeTrackingService(infoBase.ConnectionString);
            if (!IsDatabaseChangeTrackingEnabled)
            {
                service.EnableDatabaseChangeTracking(infoBase, null);
            }
            service.SwitchTableChangeTracking(entity.MainTable, true);

            publisher = Publisher.Select(entity.Identity);
            if (publisher == null)
            {
                publisher = (Publisher)IntegratorPersistentContext.Current.Factory.New(typeof(Publisher), entity.Identity);
                publisher.Name = string.Format("{0} ({1})", entity.Name, entity.MainTable.Name);
                publisher.LastSyncVersion = 0;
                publisher.Save();
            }
        }
        private void DisableChangeTracking()
        {
            ChangeTrackingService service = new ChangeTrackingService(infoBase.ConnectionString);
            service.SwitchTableChangeTracking(entity.MainTable, false);
            if (publisher != null)
            {
                publisher.Kill();
                publisher = null;
            }
        }

        public string LastSyncVersion
        {
            get { return (publisher == null) ? string.Empty : publisher.LastSyncVersion.ToString(); }
            set
            {
                if (publisher == null) return;
                publisher.LastSyncVersion = long.Parse(value);
                publisher.Save();
                OnPropertyChanged("LastSyncVersion");
            }
        }
        public string MSMQTargetQueue
        {
            get { return (publisher == null) ? string.Empty : publisher.MSMQTargetQueue; }
            set
            {
                if (publisher == null) return;
                publisher.MSMQTargetQueue = value;
                publisher.Save();
                OnPropertyChanged("MSMQTargetQueue");
            }
        }

        public ICommand PublishChangesCommand { get; private set; }
        private void OnPublishChanges()
        {
            if (publisher == null) return;
            Integrator.Services.IntegratorService service = new Integrator.Services.IntegratorService();
            int messagesSent = service.PublishChanges(publisher);
            this.NotificationRequest.Raise(new Notification
            {
                Content = string.Format("Публикация выполнена успешно.\nОпубликовано {0} сообщений.", messagesSent),
                Title = "Z-Integrator © 2016"
            });
        }
        public ICommand ProcessMessagesCommand { get; private set; }
        private void OnProcessMessages()
        {
            if (publisher == null) return;
            Integrator.Services.IntegratorService service = new Integrator.Services.IntegratorService();
            int messagesProcessed = service.ProcessMessages(publisher);
            this.NotificationRequest.Raise(new Notification
            {
                Content = string.Format("Чтение выполнено успешно.\nПрочитано {0} сообщений.", messagesProcessed),
                Title = "Z-Integrator © 2016"
            });
        }
    }
}
