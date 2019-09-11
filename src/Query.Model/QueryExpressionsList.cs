using System;
using System.Collections;
using System.Collections.Generic;

namespace OneCSharp.Query.Model
{
    public sealed class QueryExpressionsList<T> : QueryExpression, IEnumerable<T> where T : QueryExpression
    {
        private readonly List<T> _items = new List<T>();
        public QueryExpressionsList(QuerySyntaxTree consumer) : base(consumer) { }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        T this[int index]
        {
            get { return _items[index]; }
            set { _items[index] = value; }
        }
        public int Count() { return _items.Count; }
        public void Add(T parameter)
        {
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));
            _items.Add(parameter);
        }
        public void Insert(int index, T parameter)
        {
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));
            _items.Insert(index, parameter);
        }
        public void Remove(T parameter)
        {
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));
            _items.Remove(parameter);
        }
        public void RemoveAt(int index) { _items.RemoveAt(index); }
    }
}
