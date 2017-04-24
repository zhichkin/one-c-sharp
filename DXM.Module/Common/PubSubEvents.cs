using Microsoft.Practices.Prism.PubSubEvents;
using Zhichkin.DXM.Model;

namespace Zhichkin.DXM.Module
{
    public class ArticlesTreeViewItemSelected : PubSubEvent<Article> { }
}