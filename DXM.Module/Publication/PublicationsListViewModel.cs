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
    public class PublicationsListViewModel : BindableBase
    {
        private readonly InfoBase _publisher;
        private readonly IUnityContainer _container;
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;

        private object _SelectedItem = null;
        private ObservableCollection<Publication> _Publications = null;

        public PublicationsListViewModel(InfoBase publisher, IUnityContainer container, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            if (publisher == null) throw new ArgumentNullException("publisher");
            if (container == null) throw new ArgumentNullException("container");
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            _publisher = publisher;
            _container = container;
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            InitializeViewModel();
        }
        public void InitializeViewModel()
        {
            this.AddPublicationCommand = new DelegateCommand(this.OnAddPublicationCommand);
            this.BrowsePublicationCommand = new DelegateCommand<Publication>(this.OnBrowsePublication);
            this.DeletePublicationCommand = new DelegateCommand<Publication>(this.OnDeletePublication);
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
        public ICommand AddPublicationCommand { get; private set; }
        public ICommand BrowsePublicationCommand { get; private set; }
        public ICommand DeletePublicationCommand { get; private set; }
        private void OnAddPublicationCommand()
        {
            Z.Confirm(new Confirmation
                {
                    Title = Utilities.PopupDialogsTitle,
                    Content = "Создать новый план обмена (публикацию) ?"
                },
                c => { if (c.Confirmed) CreatePublication(); });
        }
        private void CreatePublication()
        {
            try
            {
                Publication publication = Publication.Create(_publisher);
                publication.Name = "Новый план обмена (публикация)";
                publication.Save();
                _Publications.Add(publication);
            }
            catch(Exception ex)
            {
                Z.Notify(new Notification { Title = Utilities.PopupDialogsTitle, Content = ExceptionsHandling.GetErrorText(ex) });
            }
        }
        private void OnBrowsePublication(Publication publication)
        {
            Z.Notify(new Notification { Title = Utilities.PopupDialogsTitle, Content = "Browse: " + publication.Name });
        }
        private void OnDeletePublication(Publication publication)
        {
            Z.Notify(new Notification { Title = Utilities.PopupDialogsTitle, Content = "Delete: " + publication.Name });
        }
    }
}
