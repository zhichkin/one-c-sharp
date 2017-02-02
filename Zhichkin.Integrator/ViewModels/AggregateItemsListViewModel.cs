using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Commands;
using Zhichkin.Metadata.Model;
using Zhichkin.ORM;
using Zhichkin.Integrator.Model;
using Zhichkin.Integrator.Services;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Zhichkin.Shell;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace Zhichkin.Integrator.ViewModels
{
    public class AggregateItemsListViewModel : BindableBase, IInteractionRequestAware
    {
        private const string CONST_ModuleDialogsTitle = "Z-Integrator";

        private Entity _aggregate;
        private Notification _notification;
        private ObservableCollection<AggregateItem> _aggregateItems = new ObservableCollection<AggregateItem>();
        private AggregateItem _selectedItem = null;
        
        public AggregateItemsListViewModel()
        {
            this.MetadataSelectionPopupRequest = new InteractionRequest<Confirmation>();
            this.ShowMetadataSelectionPopup = new DelegateCommand(this.OnShowMetadataSelectionPopup);
            this.DeleteAggregateItemCommand = new DelegateCommand<AggregateItem>(this.OnDeleteAggregateItemCommand);
        }
        public INotification Notification
        {
            get
            {
                return _notification;
            }
            set
            {
                _notification = value as Notification;
                if (_notification == null) return;
                _aggregate = _notification.Content as Entity;
                this.OnPropertyChanged("AggregateViewHeaderText");
                this.OnPropertyChanged("AggregateItems");
            }
        }
        public Action FinishInteraction { get; set; }

        public InteractionRequest<Confirmation> MetadataSelectionPopupRequest { get; private set; }
        public ICommand ShowMetadataSelectionPopup { get; private set; }
        private void OnShowMetadataSelectionPopup()
        {
            Confirmation confirmation = new Confirmation()
            {
                Title = CONST_ModuleDialogsTitle,
                Content = _aggregate.InfoBase
            };
            this.MetadataSelectionPopupRequest.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    this.CreateAggregateItem(response.Content as Entity);
                }
            });
        }
        private void CreateAggregateItem(Entity component)
        {
            if (component == null) return;
            try
            {
                AggregateItem item = AggregateItem.SelectOrCreate(_aggregate, component);
                _aggregateItems.Add(item);
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification
                {
                    Title = CONST_ModuleDialogsTitle,
                    Content = ExceptionsHandling.GetErrorText(ex)
                });
            }
        }

        public Entity Aggregate { get { return _aggregate; } }
        public string AggregateViewHeaderText
        {
            get
            {
                if (_aggregate == null) return string.Empty;
                return string.Format("{0} \"{1}\"", _aggregate.Namespace.ToString(), _aggregate.Name);
            }
        }
        public ObservableCollection<AggregateItem> AggregateItems
        {
            get
            {
                _aggregateItems.Clear();
                if (_aggregate == null) return _aggregateItems;
                foreach (AggregateItem item in AggregateItem.Select(_aggregate))
                {
                    _aggregateItems.Add(item);
                }
                return _aggregateItems;
            }
        }
        public AggregateItem SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; OnPropertyChanged("SelectedItem"); }
        }
        public void SetConnector(object value)
        {
            if (_selectedItem == null) return;
            AggregateItem item = _selectedItem as AggregateItem;
            if (item == null) return;

            Property property = value as Property;
            if (property == null) return;

            try
            {
                SaveAggregateItem(item, property);
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification
                {
                    Title = CONST_ModuleDialogsTitle,
                    Content = ExceptionsHandling.GetErrorText(ex)
                });
            }
        }
        private void SaveAggregateItem(AggregateItem item, Property property)
        {
            item.Connector = property;
            item.Save();
        }

        public ICommand DeleteAggregateItemCommand { get; private set; }
        private void OnDeleteAggregateItemCommand(AggregateItem item)
        {
            if (item == null) return;
            try
            {
                DeleteAggregateItem(item);
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification
                {
                    Title = CONST_ModuleDialogsTitle,
                    Content = ExceptionsHandling.GetErrorText(ex)
                });
            }
        }
        private void DeleteAggregateItem(AggregateItem item)
        {
            item.Kill();
            _aggregateItems.Remove(item);
        }
    }
}
