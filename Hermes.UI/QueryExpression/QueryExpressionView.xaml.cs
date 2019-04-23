using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public partial class QueryExpressionView : UserControl
    {
        public QueryExpressionView(QueryExpressionViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
