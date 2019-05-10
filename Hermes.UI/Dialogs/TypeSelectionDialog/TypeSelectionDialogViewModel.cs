using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.Services;

namespace Zhichkin.Hermes.UI
{
    public class TypeSelectionDialogViewModel : DialogViewModelBase
    {
        private UIElement _View;
        private MetadataNodeViewModel _ViewModel;

        public TypeSelectionDialogViewModel() : base() { }
        public UIElement View
        {
            get { return _View; }
            set
            {
                _View = value;
                this.OnPropertyChanged("View");
            }
        }
        protected override void InitializeViewModel(object input)
        {
            List<MetadataNodeViewModel> items = new List<MetadataNodeViewModel>();

            IMetadataService metadata = new MetadataService();
            foreach (InfoBase ib in metadata.GetInfoBases())
            {
                MetadataNode node = new MetadataNode(ib);
                MetadataNodeViewModel parentVM = new MetadataNodeViewModel(null, node);
                parentVM.Children = new ObservableCollection<MetadataNodeViewModel>();

                foreach (Namespace ns in ib.Namespaces)
                {
                    if (ns.Name == "Справочник" || ns.Name == "Документ")
                    {
                        MetadataNode child = new MetadataNode(ns);
                        MetadataNodeViewModel childVM = new MetadataNodeViewModel(parentVM, child);
                        childVM.Children = new ObservableCollection<MetadataNodeViewModel>();
                        parentVM.Children.Add(childVM);

                        foreach (Entity entity in ns.Entities)
                        {
                            MetadataNode grandchild = new MetadataNode(entity);
                            MetadataNodeViewModel grandchildVM = new MetadataNodeViewModel(childVM, grandchild);
                            childVM.Children.Add(grandchildVM);
                        }
                    }
                }

                items.Add(parentVM);
            }

            _ViewModel = new MetadataNodeViewModel();
            _ViewModel.Children = new ObservableCollection<MetadataNodeViewModel>(items);
            this.View = new MetadataNodeView(_ViewModel);
        }
        protected override object GetDialogResult()
        {
            return _ViewModel.SelectedNode;
        }
    }
}
