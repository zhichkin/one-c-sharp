using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.ViewModels;

namespace Zhichkin.Integrator.Views
{
    public partial class MetadataEntitySelectionView : UserControl
    {
        private readonly MetadataEntitySelectionViewModel ViewModel;
        public MetadataEntitySelectionView()
        {
            InitializeComponent();
            this.DataContext = ViewModel = new MetadataEntitySelectionViewModel();
        }
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ViewModel.SelectedEntity = e.NewValue as Entity;
        }
        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.SelectedEntity == null) return;
            if (!(ViewModel.SelectedEntity is Entity)) return;
            ViewModel.Confirm();
        }
    }
}
