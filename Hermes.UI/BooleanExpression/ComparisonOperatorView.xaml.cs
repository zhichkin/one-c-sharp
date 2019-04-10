using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public partial class ComparisonOperatorView : UserControl
    {
        public ComparisonOperatorView(ComparisonOperatorViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
