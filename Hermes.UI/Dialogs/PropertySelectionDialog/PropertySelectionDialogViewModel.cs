using System;
using System.Collections.Generic;
using Zhichkin.Hermes.Model;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.UI
{
    public class PropertySelectionDialogViewModel : DialogViewModelBase
    {
        private UserControl _View;
        private MetadataNodeViewModel _ViewModel;
        public PropertySelectionDialogViewModel() : base() { }
        protected override void InitializeViewModel(object input)
        {
            PropertyExpressionViewModel caller = input as PropertyExpressionViewModel;
            if (caller == null) throw new ArgumentNullException();

            SelectStatementViewModel select = GetSelectStatementViewModel(caller);
            if (select == null) throw new Exception("SelectStatementViewModel not found!");

            _ViewModel = new MetadataNodeViewModel();

            List<MetadataNodeViewModel> list = new List<MetadataNodeViewModel>();
            foreach(TableExpressionViewModel table in select.Tables)
            {
                TableExpression t = table.Model as TableExpression;
                if (t == null) continue;

                MetadataNodeViewModel child = new MetadataNodeViewModel(_ViewModel, new MetadataNode(t.Entity));
                list.Add(child);

                List<MetadataNodeViewModel> properties = new List<MetadataNodeViewModel>();
                foreach (Property property in t.Entity.Properties)
                {
                    properties.Add(new MetadataNodeViewModel(child, new MetadataNode(property)));
                }
                child.Children = new ObservableCollection<MetadataNodeViewModel>(properties);
            }
            _ViewModel.Children = new ObservableCollection<MetadataNodeViewModel>(list);
            this.View = new MetadataNodeView(_ViewModel);
        }
        protected override object GetDialogResult()
        {
            MetadataNodeViewModel selectedNode = _ViewModel.SelectedNode;
            if (selectedNode == null) return null;

            return (selectedNode.Model.Metadata as Property);
        }
        public UserControl View
        {
            get { return _View; }
            set
            {
                _View = value;
                this.OnPropertyChanged("View");
            }
        }

        private SelectStatementViewModel GetSelectStatementViewModel(HermesViewModel child)
        {
            if (child is SelectStatementViewModel) return (SelectStatementViewModel)child;
            HermesViewModel parent = child.Parent;
            while (parent != null && !(parent is SelectStatementViewModel))
            {
                parent = parent.Parent;
            }
            return (parent == null) ? null : (SelectStatementViewModel)parent;
        }
    }
}
