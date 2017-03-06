using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace Zhichkin.Shell
{
    public sealed class DatabaseSettingsNotification : Confirmation
    {
        public DatabaseSettingsNotification() { }

        public string Server { get; set; }
        public string Database { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
