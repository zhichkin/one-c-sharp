using System.Windows.Controls;
using Zhichkin.Integrator.ViewModels;

namespace Zhichkin.Integrator.Views
{
    public partial class PublisherView : UserControl
    {
        public PublisherView(PublisherViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
