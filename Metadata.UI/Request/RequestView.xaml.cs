using System.Windows.Controls;
using System.Windows.Input;

namespace Zhichkin.Metadata.UI
{
    public partial class RequestView : UserControl
    {
        public RequestView()
        {
            this.DataContext = new RequestViewModel();
            InitializeComponent();
        }
        private void Name_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key != Key.Enter) return;
            TextBox box = sender as TextBox;
            if (box == null) return;

            RequestViewModel model = this.DataContext as RequestViewModel;

            model.EnterKeyIsPressed("Name", box.Text);
        }
    }
}
