using System.Windows.Controls;
using System.Windows.Input;

namespace Zhichkin.Metadata.UI
{
    public partial class PropertyForm : UserControl
    {
        public PropertyForm()
        {
            this.DataContext = new PropertyFormModel();
            InitializeComponent();
        }
        private void Name_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key != Key.Enter) return;
            TextBox box = sender as TextBox;
            if (box == null) return;

            PropertyFormModel model = this.DataContext as PropertyFormModel;

            model.EnterKeyIsPressed("Name", box.Text);
        }
    }
}
