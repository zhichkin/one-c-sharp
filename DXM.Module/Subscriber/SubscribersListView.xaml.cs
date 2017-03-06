using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Zhichkin.Metadata.Model;

namespace Zhichkin.DXM.Module
{
    public partial class SubscribersListView : UserControl
    {
        public SubscribersListView(SubscribersListViewModel viewModel)
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
        private void View_DragEnter(object sender, DragEventArgs e)
        {
            HighlightBackground(sender);
        }
        private void View_DragLeave(object sender, DragEventArgs e)
        {
            SetDefaultBackground(sender);
        }
        private void View_Drop(object sender, DragEventArgs e)
        {
            SetDefaultBackground(sender);
            object data = e.Data.GetData(typeof(InfoBase));
            if (data == null) return;
            InfoBase item = data as InfoBase;
            if (item == null) return;
            SubscribersListViewModel viewModel = this.DataContext as SubscribersListViewModel;
            if (viewModel == null) return;
            viewModel.OnDrop(item);
        }
    }
}
