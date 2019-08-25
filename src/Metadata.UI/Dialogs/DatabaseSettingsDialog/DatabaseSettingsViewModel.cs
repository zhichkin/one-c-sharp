using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Windows.Input;

namespace Zhichkin.Metadata.UI
{
    public sealed class DatabaseSettingsViewModel : BindableBase, IInteractionRequestAware
    {
        private DatabaseSettingsNotification _notification;

        public DatabaseSettingsViewModel()
        {
            this.ConfirmCommand = new DelegateCommand(this.Confirm);
            this.CancelCommand = new DelegateCommand(this.Cancel);
        }
        public ICommand ConfirmCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        // Both the FinishInteraction and Notification properties will be set
        // by the PopupWindowAction when the popup is shown.
        public Action FinishInteraction { get; set; }
        public INotification Notification
        {
            get
            {
                return _notification;
            }
            set
            {
                if (value is DatabaseSettingsNotification)
                {
                    // To keep the code simple, this is the only property where we are raising the PropertyChanged event,
                    // as it's required to update the bindings when this property is populated.
                    // Usually you would want to raise this event for other properties too.
                    _notification = value as DatabaseSettingsNotification;
                    this.OnPropertyChanged(() => this.Notification);
                }
            }
        }

        public void Confirm()
        {
            if (_notification != null)
            {
                _notification.Confirmed = true;
            }
            this.FinishInteraction();
        }
        public void Cancel()
        {
            if (_notification != null)
            {
                _notification.Confirmed = false;
            }
            this.FinishInteraction();
        }
    }
}