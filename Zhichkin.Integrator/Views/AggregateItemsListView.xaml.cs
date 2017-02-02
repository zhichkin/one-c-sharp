using System.Windows.Controls;
using Zhichkin.Integrator.ViewModels;

namespace Zhichkin.Integrator.Views
{
    public partial class AggregateItemsListView : UserControl
    {
        private readonly AggregateItemsListViewModel _viewModel;
        public AggregateItemsListView()
        {
            InitializeComponent();
            this.DataContext = this._viewModel = new AggregateItemsListViewModel();
        }
        private void Connector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox == null) return;
            if (comboBox.SelectedItem == null) return;
            _viewModel.SetConnector(comboBox.SelectedItem);
        }
    }
}
