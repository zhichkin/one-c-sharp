using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public partial class PropertySelectionDialogView : UserControl
    {
        public PropertySelectionDialogView()
        {
            InitializeComponent();
            this.DataContext = new PropertySelectionDialogViewModel();
        }
    }
}
