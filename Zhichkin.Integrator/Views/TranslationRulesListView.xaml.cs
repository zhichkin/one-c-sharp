using System.Windows.Controls;
using Zhichkin.Integrator.ViewModels;

namespace Zhichkin.Integrator.Views
{
    public partial class TranslationRulesListView : UserControl
    {
        private readonly TranslationRulesListViewModel viewModel;
        public TranslationRulesListView(TranslationRulesListViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            this.DataContext = this.viewModel;
        }
        private void TargetProperty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox == null) return;
            if (comboBox.SelectedItem == null) return;
            viewModel.SetTargetProperty(comboBox.SelectedItem);
        }
        private void IsSyncKey_ValueChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox == null) return;
            if (comboBox.SelectedItem == null) return;
            viewModel.SetTargetProperty(comboBox.SelectedItem);
        }
    }
}
