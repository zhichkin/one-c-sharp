using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;
using Zhichkin.Hermes.Infrastructure;

namespace Zhichkin.Hermes.UI
{
    public class MetadataTreeViewModel : BindableBase
    {
        public MetadataTreeViewModel() { this.Nodes = new ObservableCollection<MetadataTreeNode>(); }
        public ObservableCollection<MetadataTreeNode> Nodes { get; set; }
    }
}
