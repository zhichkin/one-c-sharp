using System.Windows.Controls;
using Zhichkin.Integrator.ViewModels;

namespace Zhichkin.Integrator.Views
{
    public partial class InfoBaseView : UserControl
    {
        public InfoBaseView(InfoBaseViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
