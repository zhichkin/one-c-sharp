using System.Windows.Controls;
using Zhichkin.Shell;

namespace Zhichkin.DXM.Module
{
    public partial class FiltrationPropertiesView : UserControl
    {
        private readonly FiltrationPropertiesViewModel _viewModel;
        public FiltrationPropertiesView(FiltrationPropertiesViewModel viewModel)
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
