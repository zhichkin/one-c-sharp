using Syncfusion.Data;
using Syncfusion.UI.Xaml.Controls.DataPager;
using Syncfusion.UI.Xaml.Grid;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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
        }
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Items") BuildColumns();
            if (e.PropertyName == "Filter") ViewModel_FilterChanged();
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
        private async void MyDataPager_OnDemandLoading(object sender, OnDemandLoadingEventArgs e)
        {
            int itemsCount = ViewModel.GetItemsCount();
            int count = itemsCount / MyDataPager.PageSize;
            int tail = itemsCount % MyDataPager.PageSize;
            MyDataPager.PageCount = count + (tail > 0 ? 1 : 0);

            int pageNumber = e.StartIndex / MyDataPager.PageSize + 1;

            List<dynamic> list = await ViewModel.GetItemsAsync(pageNumber, MyDataPager.PageSize);
            if (list.Count == 0) return;
            MyDataPager.LoadDynamicItems(0, list);
            (MyDataPager.PagedSource as PagedCollectionView).ResetCache();
        }
        private void ViewModel_FilterChanged()
        {
            MyDataPager_OnDemandLoading(MyDataPager, new OnDemandLoadingEventArgs());
            MyDataPager.MoveToPage(0);
        }
    }
}
