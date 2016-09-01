using Microsoft.Practices.Prism.PubSubEvents;
using Zhichkin.Metadata.ViewModels;

namespace Zhichkin.Metadata.Controllers
{
    public class OpenMetadataClicked : PubSubEvent<object> { }
    public class MainMenuSaveClicked : PubSubEvent<MetadataTreeViewModel> { }
    public class ImportSQLMetadataClicked : PubSubEvent<object> { }
    public class MainMenuCommandClicked : PubSubEvent<object> { }
}
