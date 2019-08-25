using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public partial class PropertyExpressionView : UserControl
    {
        public PropertyExpressionView()
        {
            InitializeComponent();
        }
        public PropertyExpressionView(PropertyExpressionViewModel viewModel) : this()
        {
            this.DataContext = viewModel;
        }
    }
}
