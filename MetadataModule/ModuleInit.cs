using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Zhichkin.Shell;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.Views;

namespace Zhichkin.Metadata
{
    [Module(ModuleName = PersistentContext.ModuleName)]
    public class ModuleInit : IModule
    {
        private readonly IRegionViewRegistry regions;

        public ModuleInit(IRegionViewRegistry registry)
        {
            this.regions = registry;
        }

        public void Initialize()
        {
            regions.RegisterViewWithRegion(RegionNames.TopRegion, typeof(MetadataTreeView));
            regions.RegisterViewWithRegion(RegionNames.TopRegion, typeof(MetadataTreeView1));
            regions.RegisterViewWithRegion(RegionNames.LeftRegion, typeof(MetadataTreeView));
        }
    }
}
