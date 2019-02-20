using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public partial class QueryView : UserControl
    {
        public QueryView(QueryViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
