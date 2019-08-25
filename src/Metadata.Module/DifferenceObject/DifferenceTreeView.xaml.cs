using System.Windows.Controls;

namespace Zhichkin.Metadata.Module
{
    public partial class DifferenceTreeView : UserControl
    {
        public DifferenceTreeView(DifferenceTreeViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
