using System.Windows.Controls;

namespace Zhichkin.DXM.Module
{
    public partial class PublicationPropertiesView : UserControl
    {
        public PublicationPropertiesView(PublicationPropertiesViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
