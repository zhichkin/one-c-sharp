using System.Windows.Controls;

namespace Zhichkin.DXM.Module
{
    public partial class RoutingPropertiesView : UserControl
    {
        public RoutingPropertiesView(RoutingPropertiesViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
