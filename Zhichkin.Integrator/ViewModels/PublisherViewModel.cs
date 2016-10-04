using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Zhichkin.ChangeTracking;
using Zhichkin.Metadata.Model;
using Zhichkin.Integrator.Model;
using Zhichkin.Integrator.Views;
using Zhichkin.Integrator.Services;
using Microsoft.Practices.Unity;
using Zhichkin.Shell;

namespace Zhichkin.Integrator.ViewModels
{
    public class PublisherViewModel : BindableBase
    {
        private const string CONST_ModuleDialogsTitle = "Z-Integrator";

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
            
            this.UpdateTextBoxSourceCommand = new DelegateCommand<object>(this.OnUpdateTextBoxSource);
            this.QueueTestCommand = new DelegateCommand(this.OnQueueTest);
            this.CountChangesCommand = new DelegateCommand(this.OnCountChanges);
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

        public string InfoBase
        {
            get { return (entity == null) ? string.Empty : entity.InfoBase.Name; }
        }
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
            catch (Exception ex)
            {
                _ChangeTrackingDatabaseInfo = null;
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            }
            if (_ChangeTrackingDatabaseInfo == null) return;

            try
            {
                _ChangeTrackingTableInfo = services.GetChangeTrackingTableInfo(entity.MainTable);
            }
            catch (Exception ex)
            {
                _ChangeTrackingTableInfo = null;
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            }
            if (_ChangeTrackingTableInfo == null) return;

            try
            {
                publisher = Publisher.SelectOrCreate(entity);
                _SubscriptionsListView = (SubscriptionsListView)this.container.Resolve(
                    typeof(SubscriptionsListView),
                    new ParameterOverride("publisher", publisher)
                        .OnType(typeof(SubscriptionsListViewModel)));
            }
            catch (Exception ex)
            {
                publisher = null;
                _SubscriptionsListView = null;
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            }
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
                        if (Subscription.Select(publisher).Count > 0)
                        {
                            Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = "Список получателей данных не равен нулю!" });
                            return;
                        }
                        Z.Confirm(new Confirmation
                        {
                            Title = CONST_ModuleDialogsTitle,
                            Content = string.Format(
                                "Регистрация изменений и обмен данными для объекта\n\"{0}\"\nбудут полностью прекращены!\nВы уверены, что хотите продолжить?",
                                entity.FullName)
                        },
                        c => { if (c.Confirmed) DisableChangeTracking(); });
                    }
                    else
                    {
                        EnableChangeTracking();
                    }
                    InitializeViewModel();
                    OnPropertyChanged("IsChangeTrackingEnabled");
                    OnPropertyChanged("LastSyncVersion");
                    OnPropertyChanged("MSMQTargetQueue");
                    OnPropertyChanged("SubscriptionsListView");
                }
                catch (Exception ex)
                {
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                }
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
                catch (Exception ex)
                {
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                }
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
            ChangeTrackingTableInfo info = service.GetChangeTrackingTableInfo(entity.MainTable);
            if (info == null) throw new OperationCanceledException("Произошла неожиданная ошибка включения регистрации изменений!");
            publisher = Publisher.SelectOrCreate(entity);
            publisher.LastSyncVersion = info.BEGIN_VERSION;
            publisher.Save();
            _SubscriptionsListView = (SubscriptionsListView)this.container.Resolve(
                    typeof(SubscriptionsListView),
                    new ParameterOverride("publisher", publisher)
                        .OnType(typeof(SubscriptionsListViewModel)));
        }
        private void DisableChangeTracking()
        {
            ChangeTrackingService service = new ChangeTrackingService(infoBase.ConnectionString);
            service.SwitchTableChangeTracking(entity.MainTable, false);
            IntegratorService integrator = new IntegratorService();
            integrator.DeletePublisher(publisher);
            integrator.DeleteQueue(publisher);
        }

        public string LastSyncVersion
        {
            get { return (publisher == null) ? string.Empty : publisher.LastSyncVersion.ToString(); }
            set
            {
                if (publisher == null) return;
                try
                {
                    publisher.LastSyncVersion = long.Parse(value);
                    publisher.Save();
                    OnPropertyChanged("LastSyncVersion");
                }
                catch (Exception ex)
                {
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                }
            }
        }
        public string MSMQTargetQueue
        {
            get { return (publisher == null) ? string.Empty : publisher.MSMQTargetQueue; }
            set
            {
                if (publisher == null) return;
                try
                {
                    publisher.MSMQTargetQueue = value;
                    publisher.Save();
                    OnPropertyChanged("MSMQTargetQueue");
                }
                catch (Exception ex)
                {
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                }
            }
        }

        public ICommand QueueTestCommand { get; private set; }
        private void OnQueueTest()
        {
            string test = "test message";
            string answer = string.Empty;
            IntegratorService service = new IntegratorService();
            try
            {
                answer = service.TestQueue(publisher, test);
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                return;
            }
            if (test == answer)
            {
                OnPropertyChanged("MSMQTargetQueue");
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = "Тест очереди прошёл успешно!" });
            }
            else
            {
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = "Тест очереди провалился!\nПопробуйте ещё раз." });
            }
        }
        public ICommand CountChangesCommand { get; private set; }
        private void OnCountChanges()
        {
            IntegratorService service = new IntegratorService();
            try
            {
                publisher.Load();
                OnPropertyChanged("LastSyncVersion");
                int count = service.CountChanges(publisher);
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle,
                    Content = string.Format("Текущее количество изменений: {0} сообщений.", count.ToString()) });
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            }
        }
    }
}
