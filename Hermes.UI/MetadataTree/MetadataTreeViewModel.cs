using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using Zhichkin.Hermes.Infrastructure;

namespace Zhichkin.Hermes.UI
{
    public class MetadataTreeViewModel : BindableBase
    {
        public MetadataTreeViewModel()
        {
            this.SelectedDate = DateTime.Now;
            this.Nodes = new ObservableCollection<MetadataTreeNode>();
            this.StateList = new ObservableCollection<string>();
        }
        public ObservableCollection<MetadataTreeNode> Nodes { get; set; }
        public DateTime SelectedDate { get; set; }
        public void SetStateText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            this.StateList.Add(text);
        }
        public ObservableCollection<string> StateList { get; }
    }
}
