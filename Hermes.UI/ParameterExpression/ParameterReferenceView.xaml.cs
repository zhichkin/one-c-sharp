using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public partial class ParameterReferenceView : UserControl
    {
        public ParameterReferenceView()
        {
            InitializeComponent();
        }
        public ParameterReferenceView(ParameterReferenceViewModel viewModel) : this()
        {
            this.DataContext = viewModel;
        }
    }
}
