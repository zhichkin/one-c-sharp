using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Zhichkin.Shell;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.Views;
using Zhichkin.Metadata.Services;
using Zhichkin.Metadata.Controllers;

namespace Zhichkin.Metadata
{
    public class ModuleInit : IModule
    {
        private readonly IUnityContainer unity;
        private readonly IRegionViewRegistry regions;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private MainMenuController mainMenuController;

        public ModuleInit(IUnityContainer container, IRegionViewRegistry registry)
        {
            this.unity = container;
            this.regions = registry;
        }

        public void Initialize()
        {
            unity.RegisterType<IMetadataService, MetadataService>();

            mainMenuController = unity.Resolve<MainMenuController>();

            regions.RegisterViewWithRegion(RegionNames.TopRegion, ()=> this.unity.Resolve<MetadataMainMenu>());

            regions.RegisterViewWithRegion(RegionNames.LeftRegion, typeof(MetadataTreeView));
        }
    }
}
