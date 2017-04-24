using System.Windows.Controls;

namespace Zhichkin.DXM.Module
{
    public partial class PublicationMainView : UserControl
    {
        public PublicationMainView(PublicationMainViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
