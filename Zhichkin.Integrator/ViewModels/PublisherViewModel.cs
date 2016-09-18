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
using Zhichkin.Integrator.Views;
using Microsoft.Practices.Unity;

namespace Zhichkin.Integrator.ViewModels
{
    public class PublisherViewModel : BindableBase
    {
        private Publisher publisher = null;
        private readonly Entity entity;
        private readonly InfoBase infoBase;
        private readonly IUnityContainer container;
        private readonly IRegionManager regionManager;
        private readonly IEventAggregator eventAggregator;

        private ChangeTrackingDatabaseInfo _ChangeTrackingDatabaseInfo = null;
        private ChangeTrackingTableInfo _ChangeTrackingTableInfo = null;
        
        public PublisherViewModel(Entity data, IUnityContainer container, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            entity = data;
            infoBase = entity.Namespace.InfoBase;
            publisher = Publisher.Select(entity.Identity);
            if (container == null) throw new ArgumentNullException("container");
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            this.container = container;
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;

            this.NotificationRequest = new InteractionRequest<INotification>();

            this.UpdateTextBoxSourceCommand = new DelegateCommand<object>(this.OnUpdateTextBoxSource);
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

            if (publisher == null) return;
            _SubscriptionsListView = (SubscriptionsListView)this.container.Resolve(typeof(SubscriptionsListView), new ParameterOverride("publisher", publisher).OnType(typeof(SubscriptionsListViewModel)));
        }
        private SubscriptionsListView _SubscriptionsListView;
        public SubscriptionsListView SubscriptionsListView
        {
            get { return _SubscriptionsListView; }
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
                publisher.Name = entity.FullName;
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
    }
}
