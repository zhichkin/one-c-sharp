using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Zhichkin.Metadata.ViewModels;

namespace Zhichkin.Metadata.Views
{
    public partial class MetadataTreeView : UserControl
    {
        public MetadataTreeView(MetadataTreeViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            MetadataTreeViewModel viewModel = this.DataContext as MetadataTreeViewModel;
            if (viewModel == null) return;
            viewModel.SelectedItemChanged(e.NewValue);
        }
    }
}
