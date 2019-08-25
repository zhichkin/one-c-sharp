using System.Windows.Controls;

namespace Zhichkin.Metadata.UI
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
