using Microsoft.Practices.Prism.PubSubEvents;

namespace Zhichkin.Metadata.Model
{
    public class MetadataTreeViewItemSelected : PubSubEvent<object> { }
    public class MetadataInfoBaseSaveClicked : PubSubEvent<InfoBase> { }
    public class MetadataInfoBaseKillClicked : PubSubEvent<InfoBase> { }
}
