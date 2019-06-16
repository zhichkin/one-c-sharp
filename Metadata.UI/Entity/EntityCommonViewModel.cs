using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Zhichkin.Metadata.Model;
using Zhichkin.Shell;

namespace Zhichkin.Metadata.UI
{
    public sealed class EntityCommonViewModel : BindableBase, IInteractionRequestAware
    {
        private Entity model;
        private Confirmation notification;
        private UIElement _View;

        public EntityCommonViewModel()
        {
            this.ConfirmCommand = new DelegateCommand(this.Confirm);
            this.CancelCommand = new DelegateCommand(this.Cancel);
        }
        public EntityCommonViewModel(Entity entity) : this()
        {
            if (entity == null) throw new ArgumentNullException("entity");
            this.model = entity;
            this.InitializeViewModel();
        }
        public UIElement View
        {
            get { return _View; }
            set
            {
                _View = value;
                this.OnPropertyChanged("View");
            }
        }
        public ICommand ConfirmCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public Action FinishInteraction { get; set; }
        public void Confirm()
        {
            try
            {
                this.SaveModel();
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = Z.GetErrorText(ex) });
                return;
            }

            if (this.notification != null)
            {
                this.notification.Confirmed = true;
                this.FinishInteraction();
            }
        }
        public void Cancel()
        {
            if (this.notification != null)
            {
                this.notification.Confirmed = false;
            }
            this.FinishInteraction();
        }
        public INotification Notification
        {
            get
            {
                return this.notification;
            }
            set
            {
                this.notification = value as Confirmation;
                if (this.notification == null) throw new ArgumentException("notification");

                this.model = value.Content as Entity;
                if (this.model == null) throw new ArgumentException("model");

                this.InitializeViewModel();
            }
        }
        private void InitializeViewModel()
        {
            //TODO: build Grid programmatically
        }
        private void SaveModel()
        {
            //TODO: build SQL commands programmatically
        }
        public ObservableCollection<MetadataPropertyViewModel> Properties { get; private set; }
    }
}
