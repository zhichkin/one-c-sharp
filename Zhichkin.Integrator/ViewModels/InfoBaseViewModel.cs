using System;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Zhichkin.Metadata.Model;
using Zhichkin.ChangeTracking;
using Zhichkin.Shell;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace Zhichkin.Integrator.ViewModels
{
    public class InfoBaseViewModel : BindableBase
    {
        private const string CONST_ModuleDialogsTitle = "Z-Integrator";
        
        private readonly InfoBase infoBase;
        private readonly IRegionManager regionManager;
        private readonly IEventAggregator eventAggregator;

        private ChangeTrackingDatabaseInfo _ChangeTrackingDatabaseInfo = null;
        private SnapshotIsolationState _SnapshotIsolationState = SnapshotIsolationState.OFF;
        private List<string> _RetentionPeriodUnits = new List<string>() { "минуты", "часы", "дни" };
        private string _RetentionPeriodUnit = "дни";

        public InfoBaseViewModel(InfoBase data, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            infoBase = data;
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            InitializeViewModel();
        }
        public string Name
        {
            get { return (infoBase == null) ? string.Empty : infoBase.Name; }
        }
        public string Server
        {
            get { return (infoBase == null) ? string.Empty : infoBase.Server; }
        }
        public string Database
        {
            get { return (infoBase == null) ? string.Empty : infoBase.Database; }
        }

        public void InitializeViewModel()
        {
            ChangeTrackingService service = new ChangeTrackingService(infoBase.ConnectionString);
            try
            {
                _SnapshotIsolationState = service.GetSnapshotIsolationState(infoBase);
                _ChangeTrackingDatabaseInfo = service.GetChangeTrackingDatabaseInfo(infoBase);
            }
            catch
            {
                _ChangeTrackingDatabaseInfo = null;
            }
            if (_ChangeTrackingDatabaseInfo != null)
            {
                _RetentionPeriodUnit = _RetentionPeriodUnits[_ChangeTrackingDatabaseInfo.RETENTION_PERIOD_UNITS - 1];
            }
        }
        private string GetErrorText(Exception ex)
        {
            string errorText = string.Empty;
            Exception error = ex;
            while (error != null)
            {
                errorText += (errorText == string.Empty) ? error.Message : Environment.NewLine + error.Message;
                error = error.InnerException;
            }
            return errorText;
        }
        public List<string> RetentionPeriodUnits { get { return _RetentionPeriodUnits; } }
        public bool IsChangeTrackingEnabled
        {
            get { return _ChangeTrackingDatabaseInfo != null; }
            set
            {
                ChangeTrackingService service = new ChangeTrackingService(infoBase.ConnectionString);
                try
                {
                    if (IsChangeTrackingEnabled)
                    {
                        service.DisableDatabaseChangeTracking(infoBase);
                    }
                    else
                    {
                        service.EnableDatabaseChangeTracking(infoBase, _ChangeTrackingDatabaseInfo);
                    }
                    InitializeViewModel();
                    OnPropertyChanged("IsChangeTrackingEnabled");
                    OnPropertyChanged("IsSnapshotIsolationEnabled");
                    OnPropertyChanged("IsAutoCleanUpEnabled");
                    OnPropertyChanged("RetentionPeriod");
                    OnPropertyChanged("RetentionPeriodUnit");
                }
                catch (Exception ex)
                {
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                }
            }
        }
        public bool IsSnapshotIsolationEnabled
        {
            get { return (_SnapshotIsolationState == SnapshotIsolationState.ON || _SnapshotIsolationState == SnapshotIsolationState.OFF_ON); }
            set
            {
                if (_SnapshotIsolationState == SnapshotIsolationState.ON_OFF || _SnapshotIsolationState == SnapshotIsolationState.OFF_ON)
                {
                    return;
                }
                ChangeTrackingService services = new ChangeTrackingService(infoBase.ConnectionString);
                try
                {
                    services.SwitchSnapshotIsolationState(infoBase, !IsSnapshotIsolationEnabled);
                    InitializeViewModel();
                    OnPropertyChanged("IsSnapshotIsolationEnabled");
                }
                catch (Exception ex)
                {
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                }
            }
        }
        public bool IsAutoCleanUpEnabled
        {
            get { return IsChangeTrackingEnabled && _ChangeTrackingDatabaseInfo.IS_AUTO_CLEANUP_ON; }
            set
            {
                _ChangeTrackingDatabaseInfo.IS_AUTO_CLEANUP_ON = value;
                ChangeTrackingService services = new ChangeTrackingService(infoBase.ConnectionString);
                try
                {
                    services.EnableDatabaseChangeTracking(infoBase, _ChangeTrackingDatabaseInfo);
                    InitializeViewModel();
                    OnPropertyChanged("IsAutoCleanUpEnabled");
                }
                catch (Exception ex)
                {
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                }
            }
        }
        public int RetentionPeriod
        {
            get { return (IsChangeTrackingEnabled ? _ChangeTrackingDatabaseInfo.RETENTION_PERIOD : 1); }
            set
            {
                _ChangeTrackingDatabaseInfo.RETENTION_PERIOD = value;
                ChangeTrackingService services = new ChangeTrackingService(infoBase.ConnectionString);
                try
                {
                    services.EnableDatabaseChangeTracking(infoBase, _ChangeTrackingDatabaseInfo);
                    InitializeViewModel();
                    OnPropertyChanged("RetentionPeriod");
                }
                catch (Exception ex)
                {
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                }
            }
        }
        public string RetentionPeriodUnit
        {
            get { return _RetentionPeriodUnit; }
            set
            {
                _RetentionPeriodUnit = value;
                _ChangeTrackingDatabaseInfo.RETENTION_PERIOD_UNITS =
                    (short)(_RetentionPeriodUnit == "минуты" ? 1 : (_RetentionPeriodUnit == "часы" ? 2 : 3));
                _ChangeTrackingDatabaseInfo.RETENTION_PERIOD_UNITS_DESC =
                    (_RetentionPeriodUnit == "минуты" ? "MINUTES" : (_RetentionPeriodUnit == "часы" ? "HOURS" : "DAYS"));
                ChangeTrackingService services = new ChangeTrackingService(infoBase.ConnectionString);
                try
                {
                    services.EnableDatabaseChangeTracking(infoBase, _ChangeTrackingDatabaseInfo);
                    InitializeViewModel();
                    OnPropertyChanged("RetentionPeriodUnit");
                }
                catch (Exception ex)
                {
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                }
            }
        }
    }
}
