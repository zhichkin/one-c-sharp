using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Zhichkin.Metadata.ViewModels;
using Zhichkin.Shell;

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
            if (result == null) return;

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

        private void ShowProperties_Clicked(object sender, RoutedEventArgs e)
        {
            MetadataTreeViewModel viewModel = this.DataContext as MetadataTreeViewModel;
            if (viewModel == null) return;
            MenuItem menu = sender as MenuItem;
            if (menu == null) return;
            viewModel.ShowProperties(menu.DataContext);
        }

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = Z.GetParent<TreeViewItem>(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = true;
                e.Handled = true;
            }
        }
        private void OpenInfoBaseView_Clicked(object sender, RoutedEventArgs e)
        {
            MetadataTreeViewModel viewModel = this.DataContext as MetadataTreeViewModel;
            if (viewModel == null) return;
            MenuItem menu = sender as MenuItem;
            if (menu == null) return;
            viewModel.OpenInfoBaseView(menu.DataContext);
        }
        private void KillInfoBase_Clicked(object sender, RoutedEventArgs e)
        {
            MetadataTreeViewModel viewModel = this.DataContext as MetadataTreeViewModel;
            if (viewModel == null) return;
            MenuItem menu = sender as MenuItem;
            if (menu == null) return;
            viewModel.KillInfoBase(menu.DataContext);
        }
    }
}
