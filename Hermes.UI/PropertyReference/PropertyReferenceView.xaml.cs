using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public partial class PropertyReferenceView : UserControl
    {
        public PropertyReferenceView()
        {
            InitializeComponent();
        }
        public PropertyReferenceView(PropertyReferenceViewModel viewModel) : this()
        {
            this.DataContext = viewModel;
        }
    }
}
