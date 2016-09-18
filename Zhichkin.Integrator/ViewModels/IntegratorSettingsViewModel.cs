using System;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Commands;
using Zhichkin.Integrator.Services;
using Zhichkin.Integrator.Model;
using System.ServiceProcess;

namespace Zhichkin.Integrator.ViewModels
{
    public class IntegratorSettingsViewModel : BindableBase
    {
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
            this.NotificationRequest = new InteractionRequest<INotification>();

            InitializePublisherServiceInfo();
            InitializeSubscriberServiceInfo();
        }
        public InteractionRequest<INotification> NotificationRequest { get; private set; }

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
