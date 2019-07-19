using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Zhichkin.Hermes.Model
{
    public interface IMetadataTreeNode
    {
        string Name { get; set; }
        IMetadataTreeNode Parent { get; set; }
        ObservableCollection<IMetadataTreeNode> Children { get; }
        object MetadataInfo { get; set; }
        BooleanOperator Filter { get; set; }
        int Count { get; set; }
    }
    public sealed class MetadataTreeNode : IMetadataTreeNode, INotifyPropertyChanged
    {
        public MetadataTreeNode()
        {
            this.Identity = Guid.NewGuid();
            this.Children = new ObservableCollection<IMetadataTreeNode>();
        }
        public Guid Identity { get; set; }
        public string Name { get; set; }
        public IMetadataTreeNode Parent { get; set; }
        public ObservableCollection<IMetadataTreeNode> Children { get; }
        public object MetadataInfo { get; set; }
        public BooleanOperator Filter { get; set; }
        public List<Guid> Keys { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private int _Count = 0;
        public int Count
        {
            get { return _Count; }
            set
            {
                _Count = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Count"));
                }
            }
        }
    }
}