using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;

namespace OneCSharp.Query.Module
{
    public sealed class Bootstrapper : IModule
    {
        private readonly IUnityContainer unity;
        private readonly IRegionManager regions;

        public Bootstrapper(IUnityContainer container, IRegionManager regionManager)
        {
            this.unity = container;
            this.regions = regionManager;
        }
        public void Initialize()
        {
            //unity.RegisterType<ISerializationService, SerializationService>();

            //MainMenuView view = this.unity.Resolve<MainMenuView>();
            //this.regions.Regions[RegionNames.TopRegion].Add(view);
        }
    }
}