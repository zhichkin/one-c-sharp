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
        protected override void InitializeViewModel()
        {
            IMetadataService metadata = new MetadataService();

            List<MetadataNodeViewModel> items = new List<MetadataNodeViewModel>();

            AddMetadataObjects(items, metadata);

            InfoBase system = new InfoBase();
            system.Name = "Primitive types";
            MetadataNode systemNode = new MetadataNode(system);
            systemNode.Children = new List<MetadataNode>();
            MetadataNodeViewModel systemVM = new MetadataNodeViewModel(null, systemNode);
            systemVM.Children = new ObservableCollection<MetadataNodeViewModel>();
            items.Add(systemVM);

            MetadataNode systemType = new MetadataNode(Entity.Int32);
            systemNode.Children.Add(systemType);
            systemVM.Children.Add(new MetadataNodeViewModel(systemVM, systemType));

            systemType = new MetadataNode(Entity.String);
            systemNode.Children.Add(systemType);
            systemVM.Children.Add(new MetadataNodeViewModel(systemVM, systemType));

            systemType = new MetadataNode(Entity.Decimal);
            systemNode.Children.Add(systemType);
            systemVM.Children.Add(new MetadataNodeViewModel(systemVM, systemType));

            systemType = new MetadataNode(Entity.Boolean);
            systemNode.Children.Add(systemType);
            systemVM.Children.Add(new MetadataNodeViewModel(systemVM, systemType));

            systemType = new MetadataNode(Entity.DateTime);
            systemNode.Children.Add(systemType);
            systemVM.Children.Add(new MetadataNodeViewModel(systemVM, systemType));

            foreach (InfoBase ib in metadata.GetInfoBases())
            {
                MetadataNode node = new MetadataNode(ib);
                node.Children = new List<MetadataNode>();
                MetadataNodeViewModel parentVM = new MetadataNodeViewModel(null, node);
                parentVM.Children = new ObservableCollection<MetadataNodeViewModel>();

                foreach (Namespace ns in ib.Namespaces)
                {
                    if (ns.Name == "Справочник" || ns.Name == "Документ")
                    {
                        MetadataNode child = new MetadataNode(ns);
                        child.Children = new List<MetadataNode>();
                        node.Children.Add(child);
                        MetadataNodeViewModel childVM = new MetadataNodeViewModel(parentVM, child);
                        childVM.Children = new ObservableCollection<MetadataNodeViewModel>();
                        parentVM.Children.Add(childVM);

                        foreach (Entity entity in ns.Entities)
                        {
                            MetadataNode grandchild = new MetadataNode(entity);
                            child.Children.Add(grandchild);
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

        private void AddMetadataObjects(List<MetadataNodeViewModel> root, IMetadataService service)
        {
            InfoBase metadata = service.GetSystemInfoBase();
            MetadataNode metadataNode = new MetadataNode(metadata);
            metadataNode.Children = new List<MetadataNode>();
            MetadataNodeViewModel metadataVM = new MetadataNodeViewModel(null, metadataNode);
            metadataVM.Children = new ObservableCollection<MetadataNodeViewModel>();

            foreach (Namespace ns in metadata.Namespaces)
            {
                MetadataNode child = new MetadataNode(ns);
                child.Children = new List<MetadataNode>();
                metadataNode.Children.Add(child);
                MetadataNodeViewModel childVM = new MetadataNodeViewModel(metadataVM, child);
                childVM.Children = new ObservableCollection<MetadataNodeViewModel>();
                metadataVM.Children.Add(childVM);

                foreach (Entity entity in ns.Entities)
                {
                    MetadataNode grandchild = new MetadataNode(entity);
                    child.Children.Add(grandchild);
                    MetadataNodeViewModel grandchildVM = new MetadataNodeViewModel(childVM, grandchild);
                    childVM.Children.Add(grandchildVM);
                }
            }
            root.Add(metadataVM);
        }
    }
}
