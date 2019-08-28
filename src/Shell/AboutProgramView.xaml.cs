using System.Windows.Controls;

namespace Zhichkin.Shell
{
    public partial class AboutProgramView : UserControl
    {
        public AboutProgramView()
        {
            InitializeComponent();
            this.DataContext = new AboutProgramViewModel();
        }
    }
}
