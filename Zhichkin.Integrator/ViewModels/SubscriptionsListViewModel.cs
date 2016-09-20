using System;
using System.Windows.Input;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Zhichkin.Integrator.Model;
using Zhichkin.Integrator.Views;
using System.Collections.ObjectModel;
using Zhichkin.Metadata.Model;
using Zhichkin.Integrator.Services;
using Zhichkin.Shell;
using Microsoft.Practices.Unity;

namespace Zhichkin.Integrator.ViewModels
{
    public class SubscriptionsListViewModel : BindableBase
    {
        private const string CONST_ModuleDialogsTitle = "Z-Integrator";

        private readonly Publisher publisher;
        private readonly IUnityContainer container;
        private readonly IRegionManager regionManager;
        private readonly IEventAggregator eventAggregator;

        public SubscriptionsListViewModel(Publisher publisher, IUnityContainer container, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (container == null) throw new ArgumentNullException("container");
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            this.publisher = publisher;
            this.container = container;
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            this.OpenSubscriptionViewCommand = new DelegateCommand(this.OnOpenSubscriptionView);
            this.DeleteSubscriptionCommand = new DelegateCommand(this.OnDeleteSubscription);
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
        public void OnDrop(Entity item)
        {
            if (item == null) return;
            OnCreateSubscription(item);
        }
        private object _SelectedItem = null;
        public object SelectedItem
        {
            get { return _SelectedItem; }
            set { _SelectedItem = value; OnPropertyChanged("SelectedItem"); }
        }
        private ObservableCollection<Subscription> subscriptions = null;
        public IList<Subscription> Subscriptions
        {
            get
            {
                if (subscriptions == null)
                {
                    IList<Subscription> list = publisher.Subscriptions;
                    subscriptions = new ObservableCollection<Subscription>(list);
                }
                return subscriptions;
            }
        }

        private void ClearRightRegion()
        {
            IRegion rightRegion = this.regionManager.Regions[RegionNames.RightRegion];
            if (rightRegion == null) return;
            foreach (object view in rightRegion.Views)
            {
                rightRegion.Remove(view);
            }
        }

        public ICommand OpenSubscriptionViewCommand { get; private set; }
        public ICommand DeleteSubscriptionCommand { get; private set; }
        private void OnOpenSubscriptionView()
        {
            if (_SelectedItem == null) return;
            Subscription subscription = _SelectedItem as Subscription;
            if (subscription == null) return;
            ClearRightRegion();
            IRegion rightRegion = this.regionManager.Regions[RegionNames.RightRegion];
            if (rightRegion == null) return;
            object view = this.container.Resolve(typeof(SubscriptionView), new ParameterOverride("subscription", subscription).OnType(typeof(SubscriptionViewModel)));
            if (view == null) return;
            rightRegion.Add(view);
        }
        private void OnDeleteSubscription()
        {
            if (_SelectedItem == null) return;
            Subscription subscription = _SelectedItem as Subscription;
            if (subscription == null) return;
            IntegratorService service = new IntegratorService();
            try
            {
                service.DeleteSubscription(subscription);
                subscriptions.Remove(subscription);
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            }
        }
        private void OnCreateSubscription(Entity entity)
        {
            IntegratorService service = new IntegratorService();
            try
            {
                Subscription subscription = service.CreateSubscription(publisher, entity);
                subscriptions.Add(subscription);
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            }
        }
    }
}
