using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public partial class BooleanOperatorView : UserControl
    {
        public BooleanOperatorView(BooleanOperatorViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
