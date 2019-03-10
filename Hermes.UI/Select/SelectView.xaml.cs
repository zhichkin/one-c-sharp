using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Zhichkin.Hermes.Infrastructure;

namespace Zhichkin.Hermes.UI
{
    public partial class SelectView : UserControl
    {
        public SelectView()
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
            SelectExpression viewModel = this.DataContext as SelectExpression;
            if (viewModel == null) return;

            //if (e.AllowedEffects == (DragDropEffects.Copy | DragDropEffects.Move))
            //{
            //}
            TableExpression table = new EntityExpression(viewModel)
            {
                Name = item.Name,
                Alias = string.Format("{0}_{1}", item.Name, item.TypeCode.ToString())
            };
            viewModel.Tables.Add(table);
        }
    }
}
