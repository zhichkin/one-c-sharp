using System.Windows;

namespace Zhichkin.Shell
{
    public partial class Shell : Window
    {
        public Shell()
        {
            InitializeComponent();
            this.DataContext = Z.ViewModel;
        }
    }
}
