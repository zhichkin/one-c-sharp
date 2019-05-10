using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public partial class TypeSelectionDialogView : UserControl
    {
        public TypeSelectionDialogView()
        {
            InitializeComponent();
            this.DataContext = new TypeSelectionDialogViewModel();
        }
    }
}
