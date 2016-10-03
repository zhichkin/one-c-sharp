using System;
using Zhichkin.ORM;
using System.Reflection;
using System.Collections.Generic;

namespace Zhichkin.Updator.Model
{
    public abstract class DifferenceItemBase<T> : IDifferenceItem<T>
        where T : IPersistent
    {
        protected DifferenceItemBase() { }
        public DifferenceItemBase(DifferenceType type, T target, T source)
        {
            Type = type;
            Target = target;
            Source = source;
            Children = new List<IDifferenceItem>();
        }
        public DifferenceType Type { private set; get; }
        public IDifferenceItem Owner { get { return null; } }
        public List<IDifferenceItem> Children { private set; get; }
        public T Source { private set; get; }
        public T Target { private set; get; }
        public string Property { private set; get; }
        public void Apply()
        {
            if (Type == DifferenceType.None) { return; }
            else if (Type == DifferenceType.Insert) { Insert(); }
            else if (Type == DifferenceType.Update) { Update(); }
            else if (Type == DifferenceType.Delete) { Delete(); }
        }
        protected abstract void Insert();
        protected abstract void Delete();
        protected virtual void Update()
        {
            Type t = Target.GetType();
            PropertyInfo p = t.GetProperty(Property);
            if (p == null) return;
            p.SetValue(Target, p.GetValue(Source));
            Target.Save();
        }
    }
}
