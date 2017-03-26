using System.Windows.Controls;

namespace Zhichkin.DXM.Module
{
    public partial class FiltrationPropertiesView : UserControl
    {
        public FiltrationPropertiesView(FiltrationPropertiesViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
