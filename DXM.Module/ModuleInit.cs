using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Zhichkin.DXM.Module;

namespace Zhichkin.DXM
{
    public class ModuleInit : IModule
    {
        private readonly IUnityContainer unity;
        private readonly IRegionManager regions;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private MetadataTreeViewController metadataTreeViewController;

        public ModuleInit(IUnityContainer container, IRegionManager regionManager)
        {
            this.unity = container;
            this.regions = regionManager;
        }
        public void Initialize()
        {
            metadataTreeViewController = unity.Resolve<MetadataTreeViewController>();
            //IntegratorMainMenuView view = this.unity.Resolve<IntegratorMainMenuView>();
            //this.regions.Regions[RegionNames.TopRegion].Add(view);
        }
    }
}
