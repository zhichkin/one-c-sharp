using System;
using System.Windows;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Zhichkin.Metadata.Model;
using Zhichkin.ChangeTracking;

namespace Zhichkin.Integrator.ViewModels
{
    public class EntityViewModel : BindableBase
    {
        private readonly Entity entity;
        private readonly InfoBase infoBase;
        private readonly IRegionManager regionManager;
        private readonly IEventAggregator eventAggregator;

        private ChangeTrackingDatabaseInfo _ChangeTrackingDatabaseInfo = null;
        private ChangeTrackingTableInfo _ChangeTrackingTableInfo = null;

        public EntityViewModel(Entity data, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            entity = data;
            infoBase = entity.Namespace.InfoBase;
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            InitializeViewModel();
        }
        public string Name
        {
            get { return (entity == null) ? string.Empty : entity.Name; }
        }
        public string MainTable
        {
            get { return (entity?.MainTable == null) ? string.Empty : entity.MainTable.Name; }
        }

        public void InitializeViewModel()
        {
            ChangeTrackingService services = new ChangeTrackingService(infoBase.ConnectionString);

            try
            {
                _ChangeTrackingDatabaseInfo = services.GetChangeTrackingDatabaseInfo(infoBase);
            }
            catch
            {
                _ChangeTrackingDatabaseInfo = null;
            }
            if (_ChangeTrackingDatabaseInfo == null) return;

            _ChangeTrackingTableInfo = services.GetChangeTrackingTableInfo(entity.MainTable);
        }
        public bool IsDatabaseChangeTrackingEnabled
        {
            get { return _ChangeTrackingDatabaseInfo != null; }
        }
        public bool IsChangeTrackingEnabled
        {
            get { return IsDatabaseChangeTrackingEnabled && _ChangeTrackingTableInfo != null; }
            set
            {
                ChangeTrackingService service = new ChangeTrackingService(infoBase.ConnectionString);
                try
                {
                    if (IsChangeTrackingEnabled)
                    {
                        service.SwitchTableChangeTracking(entity.MainTable, false);
                    }
                    else
                    {
                        if (!IsDatabaseChangeTrackingEnabled)
                        {
                            service.EnableDatabaseChangeTracking(infoBase, null);
                        }
                        service.SwitchTableChangeTracking(entity.MainTable, true);
                    }
                    InitializeViewModel();
                    OnPropertyChanged("IsChangeTrackingEnabled");
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }
        public bool IsColumnsTrackingEnabled
        {
            get { return _ChangeTrackingTableInfo != null && _ChangeTrackingTableInfo.IS_TRACK_COLUMNS_UPDATED_ON; }
            set
            {
                ChangeTrackingService services = new ChangeTrackingService(infoBase.ConnectionString);
                try
                {
                    services.SwitchTableColumnsTracking(entity.MainTable, value);
                    InitializeViewModel();
                    OnPropertyChanged("IsColumnsTrackingEnabled");
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }
    }
}
