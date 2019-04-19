using System.Windows;
using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public partial class PropertySelectionDialogView : UserControl
    {
        public PropertySelectionDialogView()
        {
            InitializeComponent();
            this.DataContext = new PropertySelectionDialogViewModel();
        }
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            PropertySelectionDialogViewModel viewModel = this.DataContext as PropertySelectionDialogViewModel;
            if (viewModel == null) return;
            viewModel.SelectedNode = (HermesViewModel)e.NewValue;
        }
    }
}
