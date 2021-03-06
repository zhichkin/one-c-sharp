﻿using Zhichkin.Metadata.Model;

namespace OneCSharp.Query.Model
{
    public sealed class QueryParameter : QueryExpression
    {
        public QueryParameter() { }
        public string Name { get; set; }
        public Entity Type { get; set; }
        public object Value { get; set; }
    }
}
