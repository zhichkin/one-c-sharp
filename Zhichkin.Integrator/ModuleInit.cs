using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Zhichkin.Shell;
using Zhichkin.Metadata.Services;
using Zhichkin.Integrator.Views;

namespace Zhichkin.Integrator
{
    public class ModuleInit : IModule
    {
        private readonly IUnityContainer unity;
        private readonly IRegionManager regions;

        public ModuleInit(IUnityContainer container, IRegionManager regionManager)
        {
            this.unity = container;
            this.regions = regionManager;
        }

        public void Initialize()
        {
            IntegratorMainMenuView view = this.unity.Resolve<IntegratorMainMenuView>();
            this.regions.Regions[RegionNames.TopRegion].Add(view);
        }
    }
}
