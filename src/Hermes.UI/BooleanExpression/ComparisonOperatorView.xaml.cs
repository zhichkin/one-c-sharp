using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public partial class ComparisonOperatorView : UserControl
    {
        public ComparisonOperatorView()
        {
            InitializeComponent();
        }
        public ComparisonOperatorView(ComparisonOperatorViewModel viewModel) : this()
        {
            this.DataContext = viewModel;
        }
    }
}
