using System.Windows.Input;
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
        private void Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }
        private void Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Cursor = null;
        }
    }
}
