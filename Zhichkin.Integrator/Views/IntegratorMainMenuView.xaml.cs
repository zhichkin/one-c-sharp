using System.Windows.Controls;
using Zhichkin.Integrator.ViewModels;

namespace Zhichkin.Integrator.Views
{
    public partial class IntegratorMainMenuView : UserControl
    {
        public IntegratorMainMenuView(IntegratorMainMenuViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
