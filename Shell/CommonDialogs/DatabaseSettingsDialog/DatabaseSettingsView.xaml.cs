using System.Windows.Controls;

namespace Zhichkin.Shell
{
    public partial class DatabaseSettingsView : UserControl
    {
        public DatabaseSettingsView()
        {
            this.DataContext = new DatabaseSettingsViewModel();
            InitializeComponent();
        }
    }
}
