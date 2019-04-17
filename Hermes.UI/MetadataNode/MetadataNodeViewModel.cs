using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.UI
{
    public class MetadataNodeViewModel : BindableBase
    {
        public string _Name = string.Empty;
        public MetadataNodeViewModel() { } // used to create root node
        public MetadataNodeViewModel(MetadataNodeViewModel parent, MetadataNode model) : base()
        {
            this.Model = model;
            this.Parent = parent;
        }
        public MetadataNode Model { get; set; }
        public MetadataNodeViewModel Parent { get; set; }
        public ObservableCollection<MetadataNodeViewModel> Children { get; set; }
        public string Name
        {
            get
            {
                if (this.Model == null) return string.Empty;
                return this.Model.Name;
            }
            set
            {
                if (this.Model != null) throw new InvalidOperationException("This is not root metadata node!");
                _Name = value;
                this.OnPropertyChanged("Name");
            }
        }
        public BitmapImage Icon
        {
            get
            {
                if (this.Model == null) return null;
                Uri uri = null;
                if (this.Model.Metadata is Entity)
                {
                    Entity entity = (Entity)this.Model.Metadata;
                    string ns = entity.Namespace.Name;
                    if (ns == "Справочник")
                    {
                        uri = new Uri(@"/Zhichkin.Hermes.UI;component/Images/Справочник.png", UriKind.Relative);
                    }
                    else if (ns == "Документ")
                    {
                        uri = new Uri(@"/Zhichkin.Hermes.UI;component/Images/Документ.png", UriKind.Relative);
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (this.Model.Metadata is Property)
                {
                    uri = new Uri(@"/Zhichkin.Hermes.UI;component/Images/Реквизит.png", UriKind.Relative);
                }
                else
                {
                    return null;
                }
                return new BitmapImage(uri);
            }
        }
        public MetadataNodeViewModel SelectedNode { get; set; }
    }
}
