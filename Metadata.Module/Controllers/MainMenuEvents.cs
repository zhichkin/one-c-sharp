using Microsoft.Practices.Prism.PubSubEvents;
using Zhichkin.Metadata.ViewModels;

namespace Zhichkin.Metadata.Controllers
{
    public class MainMenuSaveClicked : PubSubEvent<MetadataTreeViewModel> { }
    public class MainMenuKillClicked : PubSubEvent<MetadataTreeViewModel> { }
    public class MainMenuCommandClicked : PubSubEvent<object> { }
}
