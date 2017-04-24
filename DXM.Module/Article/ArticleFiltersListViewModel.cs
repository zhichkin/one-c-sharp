using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Zhichkin.DXM.Model;
using Zhichkin.Metadata.Model;
using Zhichkin.Shell;


namespace Zhichkin.DXM.Module
{
    public class ArticleFiltersListViewModel : BindableBase
    {
        private Article _article;
        private readonly IPublisherService _publisherService = new PublisherService();
        
        private object _SelectedItem = null;
        private ObservableCollection<ArticleFilter> _Filters = null;

        public ArticleFiltersListViewModel(IEventAggregator eventAggregator)
        {
            InitializeViewModel();
            eventAggregator.GetEvent<ArticlesTreeViewItemSelected>().Subscribe(this.ArticlesTreeView_ItemSelected);
        }
        public void InitializeViewModel()
        {
            this.SetValuePropertyCommand = new DelegateCommand<object>(this.OnSetValueProperty);
            this.CreateFilterCommand = new DelegateCommand(this.OnCreateFilter);
            this.DeleteFilterCommand = new DelegateCommand<ArticleFilter>(this.OnDeleteFilter);
        }
        public void ArticlesTreeView_ItemSelected(Article article)
        {
            _article = article;
            List<ArticleFilter> list = _publisherService.Select(_article);
            _Filters = new ObservableCollection<ArticleFilter>(list);
            this.OnPropertyChanged("Filters");
        }
        public ObservableCollection<ArticleFilter> Filters { get { return _Filters; } }
        public ICommand SetValuePropertyCommand { get; private set; }
        public ICommand CreateFilterCommand { get; private set; }
        public ICommand DeleteFilterCommand { get; private set; }
        public object SelectedItem
        {
            get { return _SelectedItem; }
            set { _SelectedItem = value; OnPropertyChanged("SelectedItem"); }
        }
        public void SelectedDateChanged(DateTime selectedDate)
        {
            ArticleFilter filter = _SelectedItem as ArticleFilter;
            if (filter == null) return;
            filter.Value = selectedDate;
            filter.Save();
        }
        private void OnCreateFilter()
        {
            try
            {
                if (_article == null) return;
                ArticleFilter filter = _publisherService.Create(_article);
                _Filters.Add(filter);
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = Utilities.PopupDialogsTitle, Content = ExceptionsHandling.GetErrorText(ex) });
            }
        }
        private void OnDeleteFilter(ArticleFilter filter)
        {
            if (filter == null) return;
            try
            {
                Z.Confirm(new Confirmation
                {
                    Title = Utilities.PopupDialogsTitle,
                    Content = string.Format("Свойство \"{0}\" будет удалено. Продолжить ?", filter.Property.Name)
                }, c => { if (c.Confirmed) DeleteFilter(filter); });
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = Utilities.PopupDialogsTitle, Content = ExceptionsHandling.GetErrorText(ex) });
            }
        }
        private void DeleteFilter(ArticleFilter filter)
        {
            _publisherService.Delete(filter);
            _Filters.Remove(filter);
        }
        private void OnSetValueProperty(object userControl)
        {
            TextBox textBox = userControl as TextBox;
            if (textBox == null) return;
            DependencyProperty property = TextBox.TextProperty;
            BindingExpression binding = BindingOperations.GetBindingExpression(textBox, property);
            if (binding == null) return;

            ArticleFilter entity = null;
            if (binding.DataItem is ArticleFilter)
            {
                entity = binding.DataItem as ArticleFilter;
                binding.UpdateSource();
                entity.Save();
            }
            else if (binding.DataItem is DatePicker && ((DatePicker)binding.DataItem).DataContext is ArticleFilter)
            {
                DatePicker picker = (DatePicker)binding.DataItem;
                entity = picker.DataContext as ArticleFilter;
                entity.Value = picker.SelectedDate;
                entity.Save();
            }
            if (entity == null) return;

            DependencyObject parent = textBox.GetParent<DataGridCell>();
            if (parent == null) return;
            ((DataGridCell)parent).IsEditing = false;
        }

        private static readonly List<string> _operators = new List<string>()
        {
            "Равно",
            "Не равно",
            "Содержит",
            "Больше",
            "Больше или равно",
            "Меньше",
            "Меньше или равно",
            "Между"
        };
        public List<string> FilterOperators { get { return _operators; } }
    }
}
