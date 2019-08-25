using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace Zhichkin.Shell
{
    public sealed class ShellViewModel : BindableBase
    {
        public ShellViewModel()
        {
            this.NotificationRequest = new InteractionRequest<INotification>();
            this.ConfirmationRequest = new InteractionRequest<IConfirmation>();
        }
        public InteractionRequest<INotification> NotificationRequest { get; private set; }
        public InteractionRequest<IConfirmation> ConfirmationRequest { get; private set; }
    }
}