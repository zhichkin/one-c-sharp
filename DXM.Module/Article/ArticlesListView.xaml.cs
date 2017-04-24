using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using Zhichkin.Metadata.Model;
using Zhichkin.DXM.Model;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Zhichkin.DXM.Module
{
    public partial class ArticlesListView : UserControl
    {
        private Brush background_brush;
        private bool _IsDragging = false;
        private Point _startPoint;
        private readonly IEventAggregator eventAggregator;

        public ArticlesListView(ArticlesListViewModel viewModel, IEventAggregator eventAggregator)
        {
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            this.eventAggregator = eventAggregator;

            InitializeComponent();
            this.DataContext = viewModel;
        }
        
        // Drag Target
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

        // Drag Source
        private void ArticlesTreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }
        private void ArticlesTreeView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_IsDragging)
            {
                Point position = e.GetPosition(null);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    StartDrag(e);
                }
            }
        }
        private void StartDrag(MouseEventArgs e)
        {
            Article item  = this.ArticlesTreeView.SelectedItem as Article;
            if (item == null) return;

            _IsDragging = true;
            DragDrop.DoDragDrop(this.ArticlesTreeView, item, DragDropEffects.Copy);
            _IsDragging = false;
        }

        // is raised right after LeftMouseDown and before MouseMove
        private void ArticlesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Article article = this.ArticlesTreeView.SelectedItem as Article;
            if (article == null) return;

            this.eventAggregator.GetEvent<ArticlesTreeViewItemSelected>().Publish(article);
        }
    }
}
