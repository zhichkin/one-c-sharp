using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;
using Zhichkin.Shell;

namespace Zhichkin.Hermes.UI
{
    public partial class MetadataTreeView : UserControl
    {
        public MetadataTreeView()
        {
            InitializeComponent();
        }
        private Brush background_brush;
        private void HighlightBackground(object sender)
        {
            UserControl control = sender as UserControl;
            if (control == null) return;
            background_brush = control.Background;
            control.Background = Brushes.Azure;
        }
        private void SetDefaultBackground(object sender)
        {
            UserControl control = sender as UserControl;
            if (control == null) return;
            control.Background = background_brush;
        }
        private void View_DragEnter(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData("Zhichkin.Metadata.Model.Entity");
            if (data == null) return;
            HighlightBackground(sender);
        }
        private void View_DragLeave(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData("Zhichkin.Metadata.Model.Entity");
            if (data == null) return;
            SetDefaultBackground(sender);
        }
        private void View_Drop(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData("Zhichkin.Metadata.Model.Entity");
            if (data == null) return;
            SetDefaultBackground(sender);

            Entity entity = data as Entity;
            if (entity == null) return;

            MetadataTreeViewModel viewModel = this.DataContext as MetadataTreeViewModel;
            if (viewModel == null) return;

            viewModel.BuildDataNodesTree(entity);

            Z.Notify(new Notification { Title = "Hermes", Content = "Узлы данных сформированы." });
        }
        public void SetStateText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            Dispatcher.Invoke(() =>
            {
                MetadataTreeViewModel viewModel = this.DataContext as MetadataTreeViewModel;
                if (viewModel == null) return;
                viewModel.SetStateText(text);
                int last_index = viewModel.StateList.Count - 1;
                this.StateListBox.SelectedIndex = last_index;
                this.StateListBox.ScrollIntoView(this.StateListBox.SelectedItem);
            });
        }
        public void DocumentsTreeIsBuilt(MetadataTreeNode root)
        {
            if (root == null) return;

            Dispatcher.Invoke(() =>
            {
                MetadataTreeViewModel viewModel = this.DataContext as MetadataTreeViewModel;
                if (viewModel == null) return;
                viewModel.Nodes.Add(root);
            });
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            MetadataTreeViewModel viewModel = this.DataContext as MetadataTreeViewModel;
            if (viewModel == null) return;
            viewModel.SelectedNode = (MetadataTreeNode)e.NewValue;
        }
    }
}
