using System.Windows.Controls;
using System.Windows.Input;

namespace Zhichkin.Metadata.UI
{
    public partial class EntityForm : UserControl
    {
        public EntityForm()
        {
            this.DataContext = new EntityFormModel();
            InitializeComponent();
        }
        private void Name_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key != Key.Enter) return;
            TextBox box = sender as TextBox;
            if (box == null) return;

            EntityFormModel model = this.DataContext as EntityFormModel;

            model.EnterKeyIsPressed("Name", box.Text);
        }
        private void Alias_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key != Key.Enter) return;
            TextBox box = sender as TextBox;
            if (box == null) return;

            EntityFormModel model = this.DataContext as EntityFormModel;

            model.EnterKeyIsPressed("Alias", box.Text);
        }
        private void Code_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key != Key.Enter) return;
            TextBox box = sender as TextBox;
            if (box == null) return;

            EntityFormModel model = this.DataContext as EntityFormModel;

            model.EnterKeyIsPressed("Code", box.Text);
        }
    }
}
