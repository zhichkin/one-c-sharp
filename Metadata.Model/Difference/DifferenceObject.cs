using System;
using System.Collections.Generic;
using System.Reflection;
using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public class DifferenceObject : IDifferenceObject
    {
        private IList<IDifferenceObject> _children = new List<IDifferenceObject>();
        private IDictionary<string, object> _values = new Dictionary<string, object>();

        public DifferenceObject() { }
        public DifferenceObject(IDifferenceObject parent, IPersistent target, DifferenceType difference)
        {
            Parent = parent;
            Target = target;
            Difference = difference;
        }

        public DifferenceType Difference { set; get; }
        public IDifferenceObject Parent { set; get; }
        public IPersistent Target { set; get; }
        public IList<IDifferenceObject> Children { get { return _children; } }
        public IDictionary<string, object> NewValues { get { return _values; } }
        public void Apply()
        {
            if (Difference == DifferenceType.Insert) { Insert(); }
            else if (Difference == DifferenceType.Update) { Update(); }
            else if (Difference == DifferenceType.Delete) { Delete(); }
            Difference = DifferenceType.None;
        }
        protected virtual void Insert() { Target.Save(); }
        protected virtual void Delete() { Target.Kill(); }
        protected virtual void Update()
        {
            if (_values.Count == 0) return;
            Type t = Target.GetType();
            foreach (KeyValuePair<string, object> item in _values)
            {
                PropertyInfo p = t.GetProperty(item.Key);
                if (p == null) continue;
                p.SetValue(Target, item.Value);
            }
            Target.Save();
        }
    }
}
