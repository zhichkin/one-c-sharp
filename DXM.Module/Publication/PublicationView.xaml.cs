using System.Windows.Controls;

namespace Zhichkin.DXM.Module
{
    public partial class PublicationView : UserControl
    {
        public PublicationView(PublicationViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
