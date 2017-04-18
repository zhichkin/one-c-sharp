using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using Zhichkin.Metadata.Model;
using Syncfusion.UI.Xaml.Grid;

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
        }
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Items") BuildColumns();
        }
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
                GridTextColumn column = new GridTextColumn
                {
                    AllowEditing = false,
                    HeaderText = property.Name,
                    DisplayBinding = new Binding(property.Name)
                };
                MainDataGrid.Columns.Add(column);
            }
        }
    }
}
