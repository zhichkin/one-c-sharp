using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace Zhichkin.Metadata.Notifications
{
    public sealed class SQLConnectionDialogNotification : Confirmation
    {
        public SQLConnectionDialogNotification() { }

        public string Server { get; set; }
        public string Database { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
