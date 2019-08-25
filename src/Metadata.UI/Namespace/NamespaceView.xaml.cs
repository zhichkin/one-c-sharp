using System.Windows.Controls;
using System.Windows.Input;

namespace Zhichkin.Metadata.UI
{
    public partial class NamespaceView : UserControl
    {
        public NamespaceView()
        {
            this.DataContext = new Namespace_ViewModel();
            InitializeComponent();
        }
        private void Name_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key != Key.Enter) return;
            TextBox box = sender as TextBox;
            if (box == null) return;

            Namespace_ViewModel model = this.DataContext as Namespace_ViewModel;

            model.EnterKeyIsPressed("Name", box.Text);
        }
    }
}
