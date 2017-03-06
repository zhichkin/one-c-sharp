using System;
using System.Windows.Input;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System.Collections.ObjectModel;
using Zhichkin.Metadata.Model;
using Zhichkin.Shell;
using Microsoft.Practices.Unity;
using Zhichkin.DXM.Model;

namespace Zhichkin.DXM.Module
{
    public class SubscribersListViewModel : BindableBase
    {
        private readonly InfoBase _publisher;
        private readonly IUnityContainer _container;
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;

        private object _SelectedItem = null;
        private ObservableCollection<Publication> _Publications = null;

        public SubscribersListViewModel(InfoBase publisher, IUnityContainer container, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (container == null) throw new ArgumentNullException("container");
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            _publisher = publisher;
            _container = container;
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            this.OpenSubscriptionViewCommand = new DelegateCommand(this.OnOpenSubscriptionView);
            this.DeleteSubscriptionCommand = new DelegateCommand(this.OnDeleteSubscription);
        }
        public object SelectedItem
        {
            get { return _SelectedItem; }
            set { _SelectedItem = value; OnPropertyChanged("SelectedItem"); }
        }
        public IList<Publication> Publications
        {
            get
            {
                if (_Publications == null)
                {
                    IList<Publication> list = Publication.Select(_publisher);
                    _Publications = new ObservableCollection<Publication>(list);
                }
                return _Publications;
            }
        }
        public void OnDrop(InfoBase item)
        {
            if (item == null) return;
            OnCreateSubscription(item);
        }
        private void NotifyIfPublisherEqualsPublication(ref bool cancel, Publication publication)
        {
            if (_publisher != publication) return;
            Z.Notify(new Notification
            {
                Title = Utilities.PopupDialogsTitle,
                Content = ""
            });
        }
        private void OnCreateSubscription(InfoBase entity)
        {
            //Subscription subscription = null;
            //IntegratorService service = new IntegratorService();
            //try
            //{
            //    subscription = service.CreateSubscription(publisher, entity);
            //    subscriptions.Add(subscription);
            //}
            //catch (Exception ex)
            //{
            //    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            //}
            //try
            //{
            //    Z.Confirm(new Confirmation
            //    {
            //        Title = CONST_ModuleDialogsTitle,
            //        Content = string.Format(
            //                        "Создать правила трансляции свойств объектов\nдля подписки \"{0}\" по умолчанию?\nСопоставление свойств будет выполнено по их наименованиям.",
            //                        subscription.ToString())
            //    }, c => { if (c.Confirmed) CreateDefaultTranslationRules(subscription); });
            //}
            //catch (Exception ex)
            //{
            //    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            //}
        }
        private void CreateDefaultTranslationRules(Subscription subscription)
        {
            //IntegratorService service = new IntegratorService();
            //service.CreateTranslationRules(subscription);
        }
        public ICommand OpenSubscriptionViewCommand { get; private set; }
        public ICommand DeleteSubscriptionCommand { get; private set; }
        private void OnOpenSubscriptionView()
        {
            //if (_SelectedItem == null) return;
            //Subscription subscription = _SelectedItem as Subscription;
            //if (subscription == null) return;
            //ClearRightRegion();
            //IRegion rightRegion = this.regionManager.Regions[RegionNames.RightRegion];
            //if (rightRegion == null) return;
            //object view = this.container.Resolve(typeof(SubscriptionView), new ParameterOverride("subscription", subscription).OnType(typeof(SubscriptionViewModel)));
            //if (view == null) return;
            //rightRegion.Add(view);
        }
        private void OnDeleteSubscription()
        {
            //if (_SelectedItem == null) return;
            //Subscription subscription = _SelectedItem as Subscription;
            //if (subscription == null) return;
            //try
            //{
            //    Z.Confirm(new Confirmation
            //    {
            //        Title = CONST_ModuleDialogsTitle,
            //        Content = string.Format(
            //                        "Обмен данными для подписки\n\"{0}\"\nбудет полностью прекращён!\nВы уверены, что хотите продолжить?",
            //                        subscription.ToString())
            //    }, c => { if (c.Confirmed) DeleteSubscription(subscription); });
            //}
            //catch (Exception ex)
            //{
            //    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            //}
        }
        private void DeleteSubscription(Subscription subscription)
        {
            //IntegratorService service = new IntegratorService();
            //service.DeleteSubscription(subscription);
            //subscriptions.Remove(subscription);
        }
    }
}
