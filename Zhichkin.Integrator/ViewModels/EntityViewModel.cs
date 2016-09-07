using System;
using System.Windows;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Zhichkin.ChangeTracking;
using M = Zhichkin.Metadata.Model;
using I = Zhichkin.Integrator.Model;

namespace Zhichkin.Integrator.ViewModels
{
    public class EntityViewModel : BindableBase
    {
        private I.Entity i_entity = null;
        private readonly M.Entity entity;
        private readonly M.InfoBase infoBase;
        private readonly IRegionManager regionManager;
        private readonly IEventAggregator eventAggregator;

        private ChangeTrackingDatabaseInfo _ChangeTrackingDatabaseInfo = null;
        private ChangeTrackingTableInfo _ChangeTrackingTableInfo = null;

        public EntityViewModel(M.Entity data, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            entity = data;
            infoBase = entity.Namespace.InfoBase;
            i_entity = I.Entity.Select(entity.Identity);
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
                try
                {
                    if (IsChangeTrackingEnabled)
                    {
                        DisableChangeTracking();
                    }
                    else
                    {
                        EnableChangeTracking();
                    }
                    InitializeViewModel();
                    OnPropertyChanged("IsChangeTrackingEnabled");
                    OnPropertyChanged("LastSyncVersion");
                    OnPropertyChanged("MSMQTargetQueue");
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
        private void EnableChangeTracking()
        {
            ChangeTrackingService service = new ChangeTrackingService(infoBase.ConnectionString);
            if (!IsDatabaseChangeTrackingEnabled)
            {
                service.EnableDatabaseChangeTracking(infoBase, null);
            }
            service.SwitchTableChangeTracking(entity.MainTable, true);

            i_entity = I.Entity.Select(entity.Identity);
            if (i_entity == null)
            {
                i_entity = (I.Entity)I.IntegratorPersistentContext.Current.Factory.New(typeof(I.Entity), entity.Identity);
                i_entity.Name = string.Format("{0} ({1})", entity.Name, entity.MainTable.Name);
                i_entity.LastSyncVersion = 0;
                i_entity.Save();
            }
        }
        private void DisableChangeTracking()
        {
            ChangeTrackingService service = new ChangeTrackingService(infoBase.ConnectionString);
            service.SwitchTableChangeTracking(entity.MainTable, false);
            if (i_entity != null)
            {
                i_entity.Kill();
                i_entity = null;
            }
        }

        public string LastSyncVersion
        {
            get { return (i_entity == null) ? string.Empty : i_entity.LastSyncVersion.ToString(); }
            set
            {
                if (i_entity == null) return;
                i_entity.LastSyncVersion = long.Parse(value);
                i_entity.Save();
                OnPropertyChanged("LastSyncVersion");
            }
        }
        public string MSMQTargetQueue
        {
            get { return (i_entity == null) ? string.Empty : i_entity.MSMQTargetQueue; }
            set
            {
                if (i_entity == null) return;
                i_entity.MSMQTargetQueue = value;
                i_entity.Save();
                OnPropertyChanged("MSMQTargetQueue");
            }
        }
    }
}
