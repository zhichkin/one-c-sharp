using System.Windows;
using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public partial class MetadataNodeView : UserControl
    {
        public MetadataNodeView()
        {
            InitializeComponent();
        }
        public MetadataNodeView(MetadataNodeViewModel viewModel) : this()
        {
            this.DataContext = viewModel;
        }
        
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            MetadataNodeViewModel viewModel = this.DataContext as MetadataNodeViewModel;
            if (viewModel == null) return;
            viewModel.SelectedNode = (MetadataNodeViewModel)e.NewValue;
        }
    }
}
