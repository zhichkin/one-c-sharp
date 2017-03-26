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
    public class PublicationPropertiesViewModel : BindableBase
    {
        private readonly Publication _publication;
        private readonly IUnityContainer _container;
        private readonly IPublisherService _publisherService = new PublisherService();

        private RoutingPropertiesView _RoutingPropertiesView;
        private FiltrationPropertiesView _FiltrationPropertiesView;

        public PublicationPropertiesViewModel(Publication publication, IUnityContainer container)
        {
            if (publication == null) throw new ArgumentNullException("publication");
            if (container == null) throw new ArgumentNullException("container");
            _publication = publication;
            _container = container;
            InitializeViewModel();
        }
        public void InitializeViewModel()
        {
            _RoutingPropertiesView = (RoutingPropertiesView)_container.Resolve(
                typeof(RoutingPropertiesView),
                new ParameterOverride("publication", _publication)
                    .OnType(typeof(RoutingPropertiesViewModel)));

            _FiltrationPropertiesView = (FiltrationPropertiesView)_container.Resolve(
                typeof(FiltrationPropertiesView),
                new ParameterOverride("publication", _publication)
                    .OnType(typeof(FiltrationPropertiesViewModel)));
        }
        public RoutingPropertiesView RoutingPropertiesView { get { return _RoutingPropertiesView; } }
        public FiltrationPropertiesView FiltrationPropertiesView { get { return _FiltrationPropertiesView; } }
    }
}
