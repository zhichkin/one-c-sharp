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
    public class RoutingPropertiesViewModel : BindableBase
    {
        private readonly Publication _publication;
        private readonly IUnityContainer _container;
        private readonly IPublisherService _publisherService = new PublisherService();

        private object _SelectedItem = null;
        private ObservableCollection<Publication> _Publications = null;

        public RoutingPropertiesViewModel(Publication publication, IUnityContainer container)
        {
            if (publication == null) throw new ArgumentNullException("publication");
            if (container == null) throw new ArgumentNullException("container");
            _publication = publication;
            _container = container;
            InitializeViewModel();
        }
        public void InitializeViewModel()
        {
            
        }
    }
}
