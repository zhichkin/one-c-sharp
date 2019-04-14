using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public partial class BooleanOperatorView : UserControl
    {
        public BooleanOperatorView()
        {
            InitializeComponent();
        }
        public BooleanOperatorView(BooleanOperatorViewModel viewModel) : this()
        {
            this.DataContext = viewModel;
        }
    }
}
