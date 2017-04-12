using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Metadata.UI
{
    public partial class SelectReferenceObjectDialogView : UserControl
    {
        private readonly SelectReferenceObjectDialogViewModel ViewModel;
        public SelectReferenceObjectDialogView()
        {
            InitializeComponent();
            this.DataContext = ViewModel = new SelectReferenceObjectDialogViewModel();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            this.BuildColumns();
        }
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            BuildColumns();
        }
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ViewModel.SelectedItem = e.NewValue;
        }
        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.SelectedItem == null) return;
            ViewModel.Confirm();
        }
        private void BuildColumns()
        {
            MainDataGrid.Columns.Clear();
            if (ViewModel.Metadata == null) return;
            Entity entity = ViewModel.Metadata;
            foreach (Property property in entity.Properties)
            {
                foreach (Field field in property.Fields)
                {
                    DataGridTextColumn column = new DataGridTextColumn
                    {
                        Header = field.Name,
                        Binding = new Binding(field.Name)
                    };
                    MainDataGrid.Columns.Add(column);
                }
            }
        }
    }
}
