using System;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Zhichkin.Metadata.Model;
using Zhichkin.Integrator.Model;
using Zhichkin.Integrator.Views;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Zhichkin.Shell;
using Microsoft.Practices.Unity;

namespace Zhichkin.Integrator.ViewModels
{
    public class SubscriptionViewModel : BindableBase
    {
        private readonly Subscription subscription;
        private readonly IUnityContainer container;
        private readonly IRegionManager regionManager;
        private readonly IEventAggregator eventAggregator;

        public SubscriptionViewModel(Subscription subscription, IUnityContainer container, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            if (subscription == null) throw new ArgumentNullException("subscription");
            if (container == null) throw new ArgumentNullException("container");
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            this.subscription = subscription;
            this.container = container;
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            InitializeViewModel();
            this.CloseSubscriptionViewCommand = new DelegateCommand(this.OnCloseSubscriptionView);
        }
        public void InitializeViewModel()
        {
            if (subscription == null) return;
            translationRulesListView = (TranslationRulesListView)this.container.Resolve(
                typeof(TranslationRulesListView),
                new ParameterOverride("model", subscription)
                    .OnType(typeof(TranslationRulesListViewModel)));
        }
        public ICommand CloseSubscriptionViewCommand { get; private set; }
        private void ClearRightRegion()
        {
            IRegion rightRegion = this.regionManager.Regions[RegionNames.RightRegion];
            if (rightRegion == null) return;
            foreach (object view in rightRegion.Views)
            {
                rightRegion.Remove(view);
            }
        }

        public Publisher Publisher
        {
            get { return subscription.Publisher; }
        }
        public Entity Subscriber
        {
            get { return subscription.Subscriber; }
        }
        private TranslationRulesListView translationRulesListView;
        public TranslationRulesListView TranslationRulesListView
        {
            get { return translationRulesListView; }
        }
        private void OnCloseSubscriptionView()
        {
            if (subscription == null) return;
            if (subscription.Publisher == null) return;
            if (subscription.Publisher.Entity == null) return;
            IRegion rightRegion = this.regionManager.Regions[RegionNames.RightRegion];
            if (rightRegion == null) return;
            object view = this.container.Resolve(typeof(PublisherView), new ParameterOverride("data", subscription.Publisher.Entity).OnType(typeof(PublisherViewModel)));
            if (view == null) return;
            ClearRightRegion();
            rightRegion.Add(view);
        }
    }
}
