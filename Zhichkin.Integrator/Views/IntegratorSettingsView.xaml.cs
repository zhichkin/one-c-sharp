using System.Windows.Controls;
using Zhichkin.Integrator.ViewModels;

namespace Zhichkin.Integrator.Views
{
    public partial class IntegratorSettingsView : UserControl
    {
        public IntegratorSettingsView(IntegratorSettingsViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
