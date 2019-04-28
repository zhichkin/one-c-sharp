using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.Model
{
    public class ParameterExpression : HermesModel
    {
        public ParameterExpression(HermesModel consumer) : base(consumer) { }
        public string Name { get; set; }
        public Entity Type { get; set; }
        public object Value { get; set; }
    }
}
