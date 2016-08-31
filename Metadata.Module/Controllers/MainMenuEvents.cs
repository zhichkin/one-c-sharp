using Microsoft.Practices.Prism.PubSubEvents;

namespace Zhichkin.Metadata.Controllers
{
    public class OpenMetadataClicked : PubSubEvent<object> { }
    public class ImportSQLMetadataClicked : PubSubEvent<object> { }
    public class MainMenuCommandClicked : PubSubEvent<object> { }
}
