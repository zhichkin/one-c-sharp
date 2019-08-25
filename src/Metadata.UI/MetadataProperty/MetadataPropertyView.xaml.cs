using System.Windows.Controls;

namespace Zhichkin.Metadata.UI
{
    public partial class MetadataPropertyView : UserControl
    {
        public MetadataPropertyView()
        {
            InitializeComponent();
        }
        public MetadataPropertyView(MetadataPropertyViewModel viewModel) : this()
        {
            this.DataContext = viewModel;
        }
    }
}
