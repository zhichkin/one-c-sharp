using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Windows.Input;

namespace Zhichkin.Hermes.UI
{
    public abstract class DialogViewModelBase : BindableBase, IInteractionRequestAware
    {
        private Confirmation _notification;

        public DialogViewModelBase() : base()
        {
            this.SelectCommand = new DelegateCommand(this.Confirm);
            this.CancelCommand = new DelegateCommand(this.Cancel);
        }
        protected abstract void InitializeViewModel(object input);
        protected abstract object GetDialogResult();
        public INotification Notification
        {
            get
            {
                return _notification;
            }
            set
            {
                _notification = value as Confirmation;
                InitializeViewModel(_notification.Content);
            }
        }
        public Action FinishInteraction { get; set; }
        public ICommand SelectCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public void Confirm()
        {
            if (_notification != null)
            {
                _notification.Confirmed = true;
                _notification.Content = this.GetDialogResult();
            }
            this.FinishInteraction();
        }
        public void Cancel()
        {
            if (_notification != null)
            {
                _notification.Confirmed = false;
                _notification.Content = null;
            }
            this.FinishInteraction();
        }
    }
}
