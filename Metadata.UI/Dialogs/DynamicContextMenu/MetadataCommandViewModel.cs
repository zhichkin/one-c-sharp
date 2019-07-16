using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Zhichkin.Metadata.UI
{
    public sealed class MetadataCommandViewModel
    {
        public MetadataCommandViewModel() { }
        public string Name { get; set; }
        public string Icon { get; set; }
        public ICommand Command { get; set; }
    }
}
