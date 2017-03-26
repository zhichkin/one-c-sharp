using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Zhichkin.Metadata.Model;

namespace Zhichkin.DXM.Module
{
    public partial class ArticlesListView : UserControl
    {
        public ArticlesListView(ArticlesListViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
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
        private void ArticlesListView_DragEnter(object sender, DragEventArgs e)
        {
            HighlightBackground(sender);
        }
        private void ArticlesListView_DragLeave(object sender, DragEventArgs e)
        {
            SetDefaultBackground(sender);
        }
        private void ArticlesListView_Drop(object sender, DragEventArgs e)
        {
            SetDefaultBackground(sender);
            object data = e.Data.GetData(typeof(Entity));
            if (data == null) return;
            Entity item = data as Entity;
            if (item == null) return;
            ArticlesListViewModel viewModel = this.DataContext as ArticlesListViewModel;
            if (viewModel == null) return;
            viewModel.OnDrop(item);
        }
    }
}
