using Zhichkin.Metadata.Notifications;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Windows.Input;

namespace Zhichkin.Metadata.ViewModels
{
    public sealed class SQLConnectionDialogViewModel : BindableBase, IInteractionRequestAware
    {
        private SQLConnectionDialogNotification notification;

        public SQLConnectionDialogViewModel()
        {
            this.ConfirmCommand = new DelegateCommand(this.Confirm);
            this.CancelCommand = new DelegateCommand(this.Cancel);
        }
        public ICommand ConfirmCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        // Both the FinishInteraction and Notification properties will be set by the PopupWindowAction
        // when the popup is shown.
        public Action FinishInteraction { get; set; }
        public INotification Notification
        {
            get
            {
                return this.notification;
            }
            set
            {
                if (value is SQLConnectionDialogNotification)
                {
                    // To keep the code simple, this is the only property where we are raising the PropertyChanged event,
                    // as it's required to update the bindings when this property is populated.
                    // Usually you would want to raise this event for other properties too.
                    this.notification = value as SQLConnectionDialogNotification;
                    this.OnPropertyChanged(() => this.Notification);
                }
            }
        }

        public void Confirm()
        {
            if (this.notification != null)
            {
                this.notification.Confirmed = true;
            }
            this.FinishInteraction();
        }
        public void Cancel()
        {
            if (this.notification != null)
            {
                this.notification.Confirmed = false;
            }
            this.FinishInteraction();
        }
    }
}