using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Zhichkin.Shell;
using Zhichkin.Hermes.UI;
using Zhichkin.Hermes.Services;

namespace Zhichkin.Hermes
{
    public class ModuleManager : IModule
    {
        private readonly IUnityContainer unity;
        private readonly IRegionManager regions;
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        //private MetadataMainMenuController mainMenuController;

        public ModuleManager(IUnityContainer container, IRegionManager regionManager)
        {
            this.unity = container;
            this.regions = regionManager;
        }
        public void Initialize()
        {
            unity.RegisterType<ISerializationService, SerializationService>();

            //MainMenuView view = this.unity.Resolve<MainMenuView>();
            //this.regions.Regions[RegionNames.TopRegion].Add(view);
        }
    }
}