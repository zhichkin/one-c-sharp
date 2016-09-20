using System;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace Zhichkin.Shell
{
    public static class Z
    {
        private static readonly ShellViewModel viewModel = new ShellViewModel();
        internal static ShellViewModel ViewModel { get { return viewModel; } }
        public static void Notify(INotification context)
        {
            viewModel.NotificationRequest.Raise(context);
        }
        public static void Confirm(IConfirmation context, Action<IConfirmation> callback)
        {
            viewModel.ConfirmationRequest.Raise(context, callback);
        }
    }
}
