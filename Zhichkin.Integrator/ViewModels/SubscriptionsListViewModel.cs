using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Commands;
using Zhichkin.Integrator.Model;
using System.Collections.ObjectModel;

namespace Zhichkin.Integrator.ViewModels
{
    public class SubscriptionsListViewModel : BindableBase
    {
        private readonly Publisher publisher;
        private readonly IRegionManager regionManager;
        private readonly IEventAggregator eventAggregator;

        public SubscriptionsListViewModel(Publisher publisher, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            this.publisher = publisher;
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            this.OpenSubscriptionViewCommand = new DelegateCommand(this.OnOpenSubscriptionView);
            this.DeleteSubscriptionCommand = new DelegateCommand(this.OnDeleteSubscription);
        }
        private object _CurrentItem = null;
        public object CurrentItem
        {
            get { return _CurrentItem; }
            set { _CurrentItem = value; OnPropertyChanged("CurrentItem"); }
        }
        public IList<Subscription> Subscriptions
        {
            get
            {
                return publisher.Subscriptions;
            }
        }
        public ICommand OpenSubscriptionViewCommand { get; private set; }
        public ICommand DeleteSubscriptionCommand { get; private set; }
        private void OnOpenSubscriptionView()
        {
            MessageBox.Show(_CurrentItem.ToString());
        }
        private void OnDeleteSubscription()
        {
            MessageBox.Show(_CurrentItem.ToString());
        }
    }
}
