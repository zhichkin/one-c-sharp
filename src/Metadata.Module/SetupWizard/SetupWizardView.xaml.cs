using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using System.Windows.Controls;

namespace Zhichkin.Metadata.UI
{
    public partial class SetupWizardView : UserControl
    {
        private readonly IUnityContainer unity;
        private readonly IRegionManager manager;
        private readonly IRegionViewRegistry regions;

        public SetupWizardView(IUnityContainer container, IRegionViewRegistry registry, IRegionManager manager)
        {
            this.unity = container;
            this.manager = manager;
            this.regions = registry;

            InitializeComponent();
            this.DataContext = new SetupWizardViewModel(container, registry, manager);
        }
        
        private void SetupDatabaseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SetupWizardViewModel vm = this.DataContext as SetupWizardViewModel;
            vm.SetupDatabase(this.PassBox.Password);
        }
    }
}
