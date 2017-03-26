using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Zhichkin.DXM.Model;
using Zhichkin.Shell;

namespace Zhichkin.DXM.Module
{
    public class FiltrationPropertiesViewModel : BindableBase
    {
        private readonly Publication _publication;
        private readonly IUnityContainer _container;
        private readonly IPublisherService _publisherService = new PublisherService();

        private ObservableCollection<PublicationProperty> _Properties = null;

        public FiltrationPropertiesViewModel(Publication publication, IUnityContainer container)
        {
            if (publication == null) throw new ArgumentNullException("publication");
            if (container == null) throw new ArgumentNullException("container");
            _publication = publication;
            _container = container;
            InitializeViewModel();
        }
        public void InitializeViewModel()
        {
            this.CreatePropertyCommand = new DelegateCommand(this.OnCreateProperty);
            this.DeletePropertyCommand = new DelegateCommand<PublicationProperty>(this.OnDeleteProperty);
        }
        public ObservableCollection<PublicationProperty> Properties
        {
            get
            {
                if (_Properties == null)
                {
                    List<PublicationProperty> list = _publisherService.Select(_publication, PublicationPropertyPurpose.Filtration);
                    _Properties = new ObservableCollection<PublicationProperty>(list);
                }
                return _Properties;
            }
        }
        public ICommand CreatePropertyCommand { get; private set; }
        public ICommand DeletePropertyCommand { get; private set; }
        private void OnCreateProperty()
        {
            try
            {
                PublicationProperty property = _publisherService.Create(_publication);
                property.Name = "Новое свойство фильтрации";
                property.Purpose = PublicationPropertyPurpose.Filtration;
                property.Save();
                _Properties.Add(property);
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = Utilities.PopupDialogsTitle, Content = ExceptionsHandling.GetErrorText(ex) });
            }
        }
        private void OnDeleteProperty(PublicationProperty property)
        {
            if (property == null) return;
            try
            {
                Z.Confirm(new Confirmation
                {
                    Title = Utilities.PopupDialogsTitle,
                    Content = string.Format("Свойство \"{0}\" будет удалено. Продолжить ?", property.Name)
                }, c => { if (c.Confirmed) DeleteProperty(property); });
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = Utilities.PopupDialogsTitle, Content = ExceptionsHandling.GetErrorText(ex) });
            }
        }
        private void DeleteProperty(PublicationProperty property)
        {
            _publisherService.Delete(property);
            _Properties.Remove(property);
        }
    }
}
