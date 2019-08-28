using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using System.Windows;

namespace Zhichkin.Shell
{
    public partial class Shell : Window
    {
        public Shell(IUnityContainer container, IRegionViewRegistry regions)
        {
            InitializeComponent();
            this.DataContext = Z.ViewModel;

            regions.RegisterViewWithRegion(RegionNames.TopRegion, () => container.Resolve<ShellMainMenu>());
        }
    }
}
