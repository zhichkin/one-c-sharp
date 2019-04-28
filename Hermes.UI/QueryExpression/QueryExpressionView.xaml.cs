using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public partial class QueryExpressionView : UserControl
    {
        public QueryExpressionView()
        {
            InitializeComponent();
        }
        public QueryExpressionView(QueryExpressionViewModel viewModel) : this()
        {
            this.DataContext = viewModel;
        }
    }
}
