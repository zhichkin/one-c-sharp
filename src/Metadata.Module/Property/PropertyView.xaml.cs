using System.Windows.Controls;

namespace Zhichkin.Metadata.Module
{
    public partial class PropertyView : UserControl
    {
        public PropertyView(PropertyViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
