using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public partial class MainMenuView : UserControl
    {
        public MainMenuView(MainMenuViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
