using System.Windows.Controls;

namespace Zhichkin.Metadata.UI
{
    public partial class EntityCommonView : UserControl
    {
        public EntityCommonView()
        {
            this.DataContext = new EntityCommonViewModel();
            InitializeComponent();
        }
        public EntityCommonView(EntityCommonViewModel viewModel)
        {
            this.DataContext = viewModel;
            InitializeComponent();
        }
    }
}
