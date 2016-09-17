using System;
using System.Windows.Input;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Commands;
using Zhichkin.Integrator.Services;
using Zhichkin.Integrator.Model;

namespace Zhichkin.Integrator.ViewModels
{
    public class IntegratorSettingsViewModel : BindableBase
    {
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
            this.PublishChangesCommand = new DelegateCommand(this.OnPublishChanges);
            this.ProcessMessagesCommand = new DelegateCommand(this.OnProcessMessages);
            this.NotificationRequest = new InteractionRequest<INotification>();
        }
        public InteractionRequest<INotification> NotificationRequest { get; private set; }
        public ICommand PublishChangesCommand { get; private set; }
        private void OnPublishChanges()
        {
            IntegratorService service = new IntegratorService();
            int messagesSent = 0;
            foreach (Publisher publisher in Publisher.Select())
            {
                messagesSent += service.PublishChanges(publisher);
            }
            this.NotificationRequest.Raise(new Notification
            {
                Content = string.Format("Публикация выполнена успешно.\nОпубликовано {0} сообщений.", messagesSent),
                Title = "Z-Integrator © 2016"
            });
        }
        public ICommand ProcessMessagesCommand { get; private set; }
        private void OnProcessMessages()
        {
            IntegratorService service = new IntegratorService();
            int messagesProcessed = 0;
            foreach (Subscription subscription in service.GetSubscriptions())
            {
                messagesProcessed += service.ProcessMessages(subscription);
                
            }
            this.NotificationRequest.Raise(new Notification
            {
                Content = string.Format("Чтение выполнено успешно.\nПрочитано {0} сообщений.", messagesProcessed),
                Title = "Z-Integrator © 2016"
            });
        }
    }
}
