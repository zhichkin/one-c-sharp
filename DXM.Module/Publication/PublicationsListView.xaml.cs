using System.Windows.Controls;

namespace Zhichkin.DXM.Module
{
    public partial class PublicationsListView : UserControl
    {
        public PublicationsListView(PublicationsListViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
