using System.Windows.Controls;

namespace Zhichkin.DXM.Module
{
    public partial class ArticleFiltersListView : UserControl
    {
        private readonly ArticleFiltersListViewModel _viewModel;
        public ArticleFiltersListView(ArticleFiltersListViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = _viewModel = viewModel;
        }
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.SelectedDateChanged(((DatePicker)sender).SelectedDate.Value);
        }
    }
}
