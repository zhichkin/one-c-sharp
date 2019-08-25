using System.Windows.Controls;
using System.Windows.Input;

namespace Zhichkin.Metadata.UI
{
    public partial class InfoBaseView : UserControl
    {
        public InfoBaseView()
        {
            this.DataContext = new InfoBaseViewModel();
            InitializeComponent();
        }
        private void Name_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key != Key.Enter) return;
            TextBox box = sender as TextBox;
            if (box == null) return;

            InfoBaseViewModel model = this.DataContext as InfoBaseViewModel;

            model.EnterKeyIsPressed("Name", box.Text);
        }
        private void Server_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key != Key.Enter) return;
            TextBox box = sender as TextBox;
            if (box == null) return;

            InfoBaseViewModel model = this.DataContext as InfoBaseViewModel;

            model.EnterKeyIsPressed("Server", box.Text);
        }
        private void Database_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key != Key.Enter) return;
            TextBox box = sender as TextBox;
            if (box == null) return;

            InfoBaseViewModel model = this.DataContext as InfoBaseViewModel;

            model.EnterKeyIsPressed("Database", box.Text);
        }
        private void UserName_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key != Key.Enter) return;
            TextBox box = sender as TextBox;
            if (box == null) return;

            InfoBaseViewModel model = this.DataContext as InfoBaseViewModel;

            model.EnterKeyIsPressed("UserName", box.Text);
        }
        private void Password_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key != Key.Enter) return;
            TextBox box = sender as TextBox;
            if (box == null) return;

            InfoBaseViewModel model = this.DataContext as InfoBaseViewModel;

            model.EnterKeyIsPressed("Password", box.Text);
        }
    }
}
