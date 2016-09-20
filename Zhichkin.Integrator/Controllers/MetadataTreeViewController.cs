using System;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Zhichkin.Metadata.Services;
using Zhichkin.Metadata.Model;
using Zhichkin.Integrator.Model;
using Zhichkin.ChangeTracking;

namespace Zhichkin.Integrator.Controllers
{
    public class MetadataTreeViewController
    {
        private const string CONST_ModuleDialogsTitle = "Z-Integrator";

        private readonly IUnityContainer container;
        private readonly IRegionManager regionManager;
        private readonly IEventAggregator eventAggregator;
        private readonly IMetadataService dataService;

        public MetadataTreeViewController(IUnityContainer container,
                                  IRegionManager regionManager,
                                  IEventAggregator eventAggregator,
                                  IMetadataService dataService)
        {
            if (container == null) throw new ArgumentNullException("container");
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            if (dataService == null) throw new ArgumentNullException("dataService");
            this.container = container;
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            this.dataService = dataService;

            this.eventAggregator.GetEvent<MetadataInfoBaseKillClicked>().Subscribe(this.InfoBaseKillClicked, true);
        }

        private void InfoBaseKillClicked(InfoBase infoBase)
        {
            if (infoBase == null) throw new ArgumentNullException("infoBase");
            ChangeTrackingService services = new ChangeTrackingService(infoBase.ConnectionString);
            ChangeTrackingDatabaseInfo info = services.GetChangeTrackingDatabaseInfo(infoBase);
            if (info == null) return;
            throw new InvalidOperationException(string.Format(
                    "Удаление ИБ \"{0}\" запрещено модулем \"{1}\".\nУ этой ИБ включена регистрация изменений!",
                    infoBase.ToString(),
                    CONST_ModuleDialogsTitle));
        }
    }
}