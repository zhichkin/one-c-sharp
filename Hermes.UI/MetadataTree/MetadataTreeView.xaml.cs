using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Zhichkin.Hermes.Infrastructure;
using Zhichkin.Hermes.Services;
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

            IEntityInfo item = data as IEntityInfo;
            if (item == null) return;

            MetadataTreeViewModel viewModel = this.DataContext as MetadataTreeViewModel;
            if (viewModel == null) return;

            try
            {
                DocumentsTreeService service = new DocumentsTreeService();
                Task task = service.BuildDocumentsTree(item, viewModel.SelectedDate, SetStateText, DocumentsTreeIsBuilt);
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = Z.GetErrorText(ex) });
                return;
            }
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

            RemoveZeroCountNodes(root);

            Dispatcher.Invoke(() =>
            {
                MetadataTreeViewModel viewModel = this.DataContext as MetadataTreeViewModel;
                if (viewModel == null) return;
                viewModel.Nodes.Add(root);
            });
        }
        private void RemoveZeroCountNodes(IMetadataTreeNode root)
        {
            int index = 0;
            while (index < root.Children.Count)
            {
                IMetadataTreeNode node = root.Children[index];
                if (node.Count == 0)
                {
                    root.Children.RemoveAt(index);
                }
                else
                {
                    RemoveZeroCountNodes(node);
                    index++;
                }
            }
        }
    }
}
