using Zhichkin.Hermes.Model;
using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public class BooleanExpressionViewBuilder
    {
        private UserControl _View;
        private BooleanFunction _Model;
        public BooleanExpressionViewBuilder() { }
        public UserControl Build(BooleanFunction model)
        {
            _Model = model;
            
            // TODO

            return _View;
        }
    }
}
