using Zhichkin.ORM;
using System.Collections.Generic;

namespace Zhichkin.Metadata.Model
{
    public interface IDifferenceObject
    {
        IDifferenceObject Parent { set; get; }
        IPersistent Target { set; get; }
        IList<IDifferenceObject> Children { get; }
        DifferenceType Difference { set; get; }
        IDictionary<string, object> NewValues { get; }
        void Apply();
    }
}
