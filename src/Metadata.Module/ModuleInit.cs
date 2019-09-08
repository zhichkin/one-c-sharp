using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Zhichkin.Shell;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.Views;
using Zhichkin.Metadata.Services;
using Zhichkin.Metadata.Controllers;
using System.Configuration;
using System.Data.SqlClient;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System.Data;
using System;
using Zhichkin.Metadata.UI;

namespace Zhichkin.Metadata
{
    public class ModuleInit : IModule
    {
        private const string CONST_ModuleDialogsTitle = "Z-Metadata";
        private readonly string moduleName = MetadataPersistentContext.Current.Name;

        private readonly IUnityContainer unity;
        private readonly IRegionViewRegistry regions;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private MetadataMainMenuController mainMenuController;

        public ModuleInit(IUnityContainer container, IRegionViewRegistry registry)
        {
            this.unity = container;
            this.regions = registry;
        }
        public void Initialize()
        {
            unity.RegisterType<IMetadataService, MetadataService>();
            mainMenuController = unity.Resolve<MetadataMainMenuController>();

            MetadataPersistentContext context = (MetadataPersistentContext)MetadataPersistentContext.Current;

            if (context.CheckDatabaseConnection() && context.CheckTables())
            {
                regions.RegisterViewWithRegion(RegionNames.TopRegion, () => this.unity.Resolve<MetadataMainMenu>());
                regions.RegisterViewWithRegion(RegionNames.LeftRegion, () => this.unity.Resolve<MetadataTreeView>());
            }
            else
            {
                regions.RegisterViewWithRegion(RegionNames.RightRegion, () => this.unity.Resolve<SetupWizardView>());
            }
        }
    }
}
