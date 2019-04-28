using System.Collections.Generic;

namespace Zhichkin.Hermes.Model
{
    public class QueryExpression : HermesModel
    {
        public QueryExpression(HermesModel consumer) : base(consumer) { }
        public List<HermesModel> Expressions { get; set; }
        public List<ParameterExpression> Parameters { get; set; }
    }
}
