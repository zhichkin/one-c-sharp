using System.Windows.Controls;

namespace Zhichkin.Metadata.UI
{
    public partial class SelectParametersDialogView : UserControl
    {
        private readonly SelectParametersDialogViewModel _model;
        public SelectParametersDialogView()
        {
            InitializeComponent();
            this.DataContext = _model = new SelectParametersDialogViewModel();
        }
    }
}
