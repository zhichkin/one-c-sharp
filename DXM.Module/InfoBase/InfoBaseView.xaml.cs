using System.Windows.Controls;

namespace Zhichkin.DXM.Module
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
