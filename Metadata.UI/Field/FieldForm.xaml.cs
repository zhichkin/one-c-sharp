using System.Windows.Controls;
using System.Windows.Input;

namespace Zhichkin.Metadata.UI
{
    public partial class FieldForm : UserControl
    {
        public FieldForm()
        {
            this.DataContext = new FieldFormModel();
            InitializeComponent();
        }
        private void Name_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key != Key.Enter) return;
            TextBox box = sender as TextBox;
            if (box == null) return;

            FieldFormModel model = this.DataContext as FieldFormModel;

            model.EnterKeyIsPressed("Name", box.Text);
        }
        private void KeyOrdinal_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key != Key.Enter) return;
            TextBox box = sender as TextBox;
            if (box == null) return;

            FieldFormModel model = this.DataContext as FieldFormModel;

            model.EnterKeyIsPressed("KeyOrdinal", box.Text);
        }
        private void TypeName_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key != Key.Enter) return;
            TextBox box = sender as TextBox;
            if (box == null) return;

            FieldFormModel model = this.DataContext as FieldFormModel;

            model.EnterKeyIsPressed("TypeName", box.Text);
        }
        private void Length_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key != Key.Enter) return;
            TextBox box = sender as TextBox;
            if (box == null) return;

            FieldFormModel model = this.DataContext as FieldFormModel;

            model.EnterKeyIsPressed("Length", box.Text);
        }
        private void Precision_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key != Key.Enter) return;
            TextBox box = sender as TextBox;
            if (box == null) return;

            FieldFormModel model = this.DataContext as FieldFormModel;

            model.EnterKeyIsPressed("Precision", box.Text);
        }
        private void Scale_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key != Key.Enter) return;
            TextBox box = sender as TextBox;
            if (box == null) return;

            FieldFormModel model = this.DataContext as FieldFormModel;

            model.EnterKeyIsPressed("Scale", box.Text);
        }
    }
}
