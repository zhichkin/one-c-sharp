﻿using System.Windows.Controls;
using System.Windows.Input;

namespace Zhichkin.Metadata.UI
{
    public partial class TableForm : UserControl
    {
        public TableForm()
        {
            this.DataContext = new TableFormModel();
            InitializeComponent();
        }
        private void Name_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key != Key.Enter) return;
            TextBox box = sender as TextBox;
            if (box == null) return;

            TableFormModel model = this.DataContext as TableFormModel;

            model.EnterKeyIsPressed("Name", box.Text);
        }
        private void Schema_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key != Key.Enter) return;
            TextBox box = sender as TextBox;
            if (box == null) return;

            TableFormModel model = this.DataContext as TableFormModel;

            model.EnterKeyIsPressed("Schema", box.Text);
        }
    }
}
