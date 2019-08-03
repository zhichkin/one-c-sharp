using System;
using System.Collections.Generic;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.Model
{
    public class QueryExpression : HermesModel
    {
        public QueryExpression() : base(null) { }
        public QueryExpression(HermesModel consumer) : base(consumer) { }
        public QueryExpression(HermesModel consumer, Request request) : base(consumer)
        {
            this.Request = request ?? throw new ArgumentNullException("request");
        }

        public bool ShouldSerializeRequest() { return false; }
        public Request Request { get; set; }

        public List<HermesModel> Expressions { get; set; }
        public List<ParameterExpression> Parameters { get; set; }
    }
}
