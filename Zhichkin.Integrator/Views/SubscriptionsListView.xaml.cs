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
using Zhichkin.Integrator.ViewModels;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Integrator.Views
{
    public partial class SubscriptionsListView : UserControl
    {
        public SubscriptionsListView(SubscriptionsListViewModel viewModel)
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
            object data = e.Data.GetData(typeof(Entity));
            if (data == null) return;
            Entity item = data as Entity;
            if (item == null) return;
            SubscriptionsListViewModel viewModel = this.DataContext as SubscriptionsListViewModel;
            if (viewModel == null) return;
            viewModel.OnDrop(item);
        }
    }
}
