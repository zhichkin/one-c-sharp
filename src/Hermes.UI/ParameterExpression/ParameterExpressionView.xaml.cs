using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public partial class ParameterExpressionView : UserControl
    {
        public ParameterExpressionView()
        {
            InitializeComponent();
        }
        public ParameterExpressionView(ParameterExpressionViewModel viewModel) : this()
        {
            this.DataContext = viewModel;
        }
    }
}
