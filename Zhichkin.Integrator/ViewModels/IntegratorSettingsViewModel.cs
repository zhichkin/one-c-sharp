using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Commands;
using Zhichkin.Integrator.Model;
using System.ServiceProcess;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using Zhichkin.Shell;

namespace Zhichkin.Integrator.ViewModels
{
    public class IntegratorSettingsViewModel : BindableBase
    {
        private readonly string moduleName = IntegratorPersistentContext.Current.Name;
        private const string CONST_ModuleDialogsTitle = "Z-Integrator";
        private const string CONST_PublisherServiceName = "Z-Integrator Publisher Service";
        private const string CONST_SubscriberServiceName = "Z-Integrator Subscriber Service";

        private readonly IRegionManager regionManager;
        private readonly IEventAggregator eventAggregator;

        public IntegratorSettingsViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            InitializeViewModel();
        }
        private void InitializeViewModel()
        {
            this.PublisherServiceSwitchCommand = new DelegateCommand(this.OnPublisherServiceSwitch);
            this.SubscriberServiceSwitchCommand = new DelegateCommand(this.OnSubscriberServiceSwitch);
            InitializePublisherServiceInfo();
            InitializeSubscriberServiceInfo();
            this.UpdateTextBoxSourceCommand = new DelegateCommand<object>(this.OnUpdateTextBoxSource);
            this.CheckConnectionCommand = new DelegateCommand(this.OnCheckConnection);
            _IntegratorConnectionString = ConfigurationManager.ConnectionStrings[moduleName].ConnectionString;
        }
        public ICommand CheckConnectionCommand { get; private set; }
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

        private string _IntegratorConnectionString = string.Empty;
        public string IntegratorConnectionString
        {
            get { return _IntegratorConnectionString; }
            set
            {
                try
                {
                    UpdateIntegratorConnectionString(value);
                    _IntegratorConnectionString = value;
                    OnPropertyChanged("IntegratorConnectionString");
                }
                catch (Exception ex)
                {
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                }
            }
        }
        private void UpdateIntegratorConnectionString(string connectionString)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConnectionStringSettingsCollection settings = config.ConnectionStrings.ConnectionStrings;
            ConnectionStringSettings setting = settings[moduleName];
            if (setting == null)
            {
                settings.Add(new ConnectionStringSettings(moduleName, connectionString, "System.Data.SqlClient"));
            }
            else
            {
                setting.ConnectionString = connectionString;
            }
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("connectionStrings");

            Z.Notify(new Notification
            {
                Title = CONST_ModuleDialogsTitle,
                Content = string.Format("Настройка строки соединения для модуля \"{0}\" выполнена.", moduleName)
            });
        }
        private void OnCheckConnection()
        {
            string resultMessage = string.Empty;
            SqlConnection connection = new SqlConnection(_IntegratorConnectionString);
            try
            {
                connection.Open();
                if (connection.State == ConnectionState.Open)
                {
                    resultMessage = "Соединение открыто успешно.";
                }
                else
                {
                    resultMessage = string.Format("Соединение получило статус: \"{0}\".", connection.State.ToString());
                }
            }
            catch (Exception ex)
            {
                resultMessage = GetErrorText(ex);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
                connection.Dispose();
            }
            Z.Notify(new Notification
            {
                Title = CONST_ModuleDialogsTitle,
                Content = resultMessage
            });
        }

        private ServiceController GetPublisherServiceController()
        {
            return ServiceController.GetServices(Environment.MachineName)
                .FirstOrDefault(s => s.ServiceName == CONST_PublisherServiceName);
        }
        private void InitializePublisherServiceInfo()
        {
            ServiceController publisherService = GetPublisherServiceController();
            if (publisherService == null)
            {
                this.IsPublisherServiceInstalled = false;
                PublisherServiceStateText = "not installed";
                PublisherServiceButtonContent = "";
            }
            else
            {
                this.IsPublisherServiceInstalled = true;
                if (publisherService.Status == ServiceControllerStatus.Running)
                {
                    PublisherServiceStateText = "Running";
                    PublisherServiceButtonContent = "Stop";
                }
                else if (publisherService.Status == ServiceControllerStatus.Stopped)
                {
                    PublisherServiceStateText = "Stopped";
                    PublisherServiceButtonContent = "Start";
                }
            }
            OnPropertyChanged("IsPublisherServiceInstalled");
            OnPropertyChanged("PublisherServiceStateText");
            OnPropertyChanged("PublisherServiceButtonContent");
        }
        private bool _IsPublisherServiceInstalled = false;
        private string _PublisherServiceStateText = string.Empty;
        private string _PublisherServiceButtonContent = string.Empty;
        public ICommand PublisherServiceSwitchCommand { get; private set; }
        public bool IsPublisherServiceInstalled
        {
            get { return _IsPublisherServiceInstalled; }
            set
            {
                _IsPublisherServiceInstalled = value;
                OnPropertyChanged("IsPublisherServiceInstalled");
            }
        }
        public string PublisherServiceStateText
        {
            get { return _PublisherServiceStateText; }
            set
            {
                _PublisherServiceStateText = value;
                OnPropertyChanged("PublisherServiceStateText");
            }
        }
        public string PublisherServiceButtonContent
        {
            get
            {
                return _PublisherServiceButtonContent;
            }
            set
            {
                _PublisherServiceButtonContent = value;
                OnPropertyChanged("PublisherServiceButtonContent");
            }
        }
        private void OnPublisherServiceSwitch()
        {
            try
            {
                SwitchPublisherService();
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            }
        }
        private void SwitchPublisherService()
        {
            ServiceController publisherService = GetPublisherServiceController();
            if (publisherService == null) return;
            if (publisherService.Status == ServiceControllerStatus.Running)
            {
                publisherService.Stop();
                publisherService.WaitForStatus(ServiceControllerStatus.Stopped);
                InitializePublisherServiceInfo();
            }
            else if (publisherService.Status == ServiceControllerStatus.Stopped)
            {
                publisherService.Start();
                publisherService.WaitForStatus(ServiceControllerStatus.Running);
                InitializePublisherServiceInfo();
            }
        }

        private ServiceController GetSubscriberServiceController()
        {
            return ServiceController.GetServices(Environment.MachineName)
                .FirstOrDefault(s => s.ServiceName == CONST_SubscriberServiceName);
        }
        private void InitializeSubscriberServiceInfo()
        {
            ServiceController subscriberService = GetSubscriberServiceController();
            if (subscriberService == null)
            {
                this.IsSubscriberServiceInstalled = false;
                SubscriberServiceStateText = "not installed";
                SubscriberServiceButtonContent = "";
            }
            else
            {
                this.IsSubscriberServiceInstalled = true;
                if (subscriberService.Status == ServiceControllerStatus.Running)
                {
                    SubscriberServiceStateText = "Running";
                    SubscriberServiceButtonContent = "Stop";
                }
                else if (subscriberService.Status == ServiceControllerStatus.Stopped)
                {
                    SubscriberServiceStateText = "Stopped";
                    SubscriberServiceButtonContent = "Start";
                }
            }
            OnPropertyChanged("IsSubscriberServiceInstalled");
            OnPropertyChanged("SubscriberServiceStateText");
            OnPropertyChanged("SubscriberServiceButtonContent");
        }
        private bool _IsSubscriberServiceInstalled = false;
        private string _SubscriberServiceStateText = string.Empty;
        private string _SubscriberServiceButtonContent = string.Empty;
        public ICommand SubscriberServiceSwitchCommand { get; private set; }
        public bool IsSubscriberServiceInstalled
        {
            get { return _IsSubscriberServiceInstalled; }
            set
            {
                _IsSubscriberServiceInstalled = value;
                OnPropertyChanged("IsSubscriberServiceInstalled");
            }
        }
        public string SubscriberServiceStateText
        {
            get
            {
                return _SubscriberServiceStateText;
            }
            set
            {
                _SubscriberServiceStateText = value;
                OnPropertyChanged("SubscriberServiceStateText");
            }
        }
        public string SubscriberServiceButtonContent
        {
            get
            {
                return _SubscriberServiceButtonContent;
            }
            set
            {
                _SubscriberServiceButtonContent = value;
                OnPropertyChanged("SubscriberServiceButtonContent");
            }
        }
        private void OnSubscriberServiceSwitch()
        {
            try
            {
                SwitchSubscriberService();
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            }
        }
        private void SwitchSubscriberService()
        {
            ServiceController subscriberService = GetSubscriberServiceController();
            if (subscriberService == null) return;
            if (subscriberService.Status == ServiceControllerStatus.Running)
            {
                subscriberService.Stop();
                subscriberService.WaitForStatus(ServiceControllerStatus.Stopped);
                InitializeSubscriberServiceInfo();
            }
            else if (subscriberService.Status == ServiceControllerStatus.Stopped)
            {
                subscriberService.Start();
                subscriberService.WaitForStatus(ServiceControllerStatus.Running);
                InitializeSubscriberServiceInfo();
            }
        }
    }
}
