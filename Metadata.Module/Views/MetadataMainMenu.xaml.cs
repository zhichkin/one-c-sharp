using System.Windows.Controls;
using Zhichkin.Metadata.ViewModels;

namespace Zhichkin.Metadata.Views
{
    public partial class MetadataMainMenu : UserControl
    {
        public MetadataMainMenu(MainMenuViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
