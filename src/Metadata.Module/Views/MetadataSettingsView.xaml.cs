using System.Windows.Controls;
using Zhichkin.Metadata.ViewModels;

namespace Zhichkin.Metadata.Views
{
    public partial class MetadataSettingsView : UserControl
    {
        public MetadataSettingsView(MetadataSettingsViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
