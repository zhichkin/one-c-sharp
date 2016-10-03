using System.Collections.Generic;
using Zhichkin.ORM;

namespace Zhichkin.Updator.Model
{
    public interface IDifferenceItem
    {
        DifferenceType Type { get; }
        IDifferenceItem Owner { get; }
        List<IDifferenceItem> Children { get; }
        void Apply();
    }

    public interface IDifferenceItem<T> : IDifferenceItem
        where T : IPersistent
    {
        T Target { get; }
        T Source { get; }
    }
}
