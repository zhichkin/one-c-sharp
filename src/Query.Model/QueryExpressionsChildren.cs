using System;
using System.Collections;
using System.Collections.Generic;

namespace OneCSharp.Query.Model
{
    public sealed class QueryExpressionsChildren : QueryExpression, IEnumerable<QueryExpression>
    {
        private readonly List<QueryExpression> _children;
        public QueryExpressionsChildren()
        {
            _children = new List<QueryExpression>();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _children.GetEnumerator();
        }
        IEnumerator<QueryExpression> IEnumerable<QueryExpression>.GetEnumerator()
        {
            return _children.GetEnumerator();
        }
        QueryExpression this[int index]
        {
            get { return _children[index]; }
            set { _children[index] = value; }
        }
        public int Count() { return _children.Count; }
        public TChild AddChild<TChild>() where TChild : QueryExpression, new()
        {
            TChild child = new TChild() { Parent = this };
            _children.Add(child);
            return child;
        }
        public void Add(QueryExpression child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            _children.Add(child);
        }
        public TChild Insert<TChild>(int index) where TChild : QueryExpression, new()
        {
            TChild child = new TChild() { Parent = this };
            _children.Insert(index, child);
            return child;
        }
        public void Insert(int index, QueryExpression child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            _children.Insert(index, child);
        }
        public void Remove(QueryExpression child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            _children.Remove(child);
        }
        public void RemoveAt(int index) { _children.RemoveAt(index); }
    }
}
