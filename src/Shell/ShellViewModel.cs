using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using System.Threading.Tasks;
using Squirrel;
using System.Reflection;
using System.Diagnostics;

namespace Zhichkin.Shell
{
    public sealed class ShellViewModel : BindableBase
    {
        public ShellViewModel()
        {
            this.NotificationRequest = new InteractionRequest<INotification>();
            this.ConfirmationRequest = new InteractionRequest<IConfirmation>();

            this.CheckForUpdatesCommand = new DelegateCommand(this.OnCheckForUpdates);
            this.ShowAboutProgramViewCommand = new DelegateCommand(this.ShowAboutProgramView);
        }
        public InteractionRequest<INotification> NotificationRequest { get; private set; }
        public InteractionRequest<IConfirmation> ConfirmationRequest { get; private set; }

        public ICommand ShowAboutProgramViewCommand { get; private set; }
        private void ShowAboutProgramView()
        {
            Z.Notify(new Notification() { Title = "About 1C# © 2016", Content = new AboutProgramView()});
        }

        public ICommand CheckForUpdatesCommand { get; private set; }
        private void OnCheckForUpdates()
        {
            _ = CheckForUpdates();
        }
        private async Task CheckForUpdates()
        {
            //using (var manager = new UpdateManager(@"C:\Users\User\Desktop\GitHub\one-c-sharp\install"))
            //{
            //    await manager.UpdateApp();
            //}
        }
    }
}