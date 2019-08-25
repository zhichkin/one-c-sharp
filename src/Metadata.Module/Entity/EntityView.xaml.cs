using System.Windows.Controls;

namespace Zhichkin.Metadata.Module
{
    public partial class EntityView : UserControl
    {
        public EntityView(EntityViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
