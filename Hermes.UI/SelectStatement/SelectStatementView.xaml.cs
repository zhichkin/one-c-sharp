using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.UI
{
    public partial class SelectStatementView : UserControl
    {
        public SelectStatementView()
        {
            InitializeComponent();
        }

        private Brush background_brush;
        private void HighlightBackground(object sender)
        {
            UserControl control = sender as UserControl;
            if (control == null) return;
            background_brush = control.Background;
            //control.Background = Brushes.Azure;
            this.Expander_FROM.Background = Brushes.LightGreen;
        }
        private void SetDefaultBackground(object sender)
        {
            UserControl control = sender as UserControl;
            if (control == null) return;
            //control.Background = background_brush;
            this.Expander_FROM.Background = background_brush;
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

            SelectStatementViewModel viewModel = this.DataContext as SelectStatementViewModel;
            if (viewModel == null) return;

            viewModel.AddTableCommand.Execute(entity);
        }
    }
}
