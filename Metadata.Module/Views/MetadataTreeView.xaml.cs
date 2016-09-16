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
        private void TreeView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            TreeView control = sender as TreeView;
            if (control == null) return;

            Point point = e.GetPosition(this);
            HitTestResult result = VisualTreeHelper.HitTest(this, point);
            DependencyObject obj = result.VisualHit;
            while (VisualTreeHelper.GetParent(obj) != null && !(obj is TextBlock))
            {
                obj = VisualTreeHelper.GetParent(obj);
            }
            TextBlock item = obj as TextBlock;
            if (item == null) return;
            if (item.DataContext == null) return;

            DragDrop.DoDragDrop(control, item.DataContext, DragDropEffects.Copy);
        }

        private void TreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                e.Handled = true;
                MetadataTreeViewModel viewModel = this.DataContext as MetadataTreeViewModel;
                if (viewModel == null) return;
                TreeView item = sender as TreeView;
                if (item == null) return;
                if (item.SelectedItem == null) return;
                viewModel.TreeViewDoubleClickCommand.Execute(item.SelectedItem);
            }
        }
    }
}
