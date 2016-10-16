using System;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Commands;
using Zhichkin.Metadata.Model;
using Zhichkin.ChangeTracking;
using Zhichkin.Shell;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;

namespace Zhichkin.Integrator.ViewModels
{
    public class InfoBaseViewModel : BindableBase
    {
        private const string CONST_ModuleDialogsTitle = "Z-Integrator";
        private const string CONST_LogEntityNameTooltip = ". . .";
        private const string CONST_LogEntitySettingName = "documents_log_entity";
        private const string CONST_LogPropertySettingName = "documents_log_property";

        private readonly InfoBase infoBase;
        private readonly IRegionManager regionManager;
        private readonly IEventAggregator eventAggregator;

        private ChangeTrackingDatabaseInfo _ChangeTrackingDatabaseInfo = null;
        private SnapshotIsolationState _SnapshotIsolationState = SnapshotIsolationState.OFF;
        private List<string> _RetentionPeriodUnits = new List<string>() { "минуты", "часы", "дни" };
        private string _RetentionPeriodUnit = "дни";

        public InfoBaseViewModel(InfoBase data, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            infoBase = data;
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            InitializeViewModel();
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
            this.UpdateTextBoxSourceCommand = new DelegateCommand<object>(this.OnUpdateTextBoxSource);

            this.SelectLogEntityCommand = new DelegateCommand(this.OnSelectLogEntity);
            this.ClearLogEntityCommand = new DelegateCommand(this.OnClearLogEntity);
            this.SelectEntityPopupRequest = new InteractionRequest<Notification>();
            InitializeCustomSettings();
        }
        private void InitializeCustomSettings()
        {
            this.LogEntityName = CONST_LogEntityNameTooltip;
            _LogEntityProperty = string.Empty;
            this.LogEntityProperties = new List<string>();
            this.IsLogEntityEnabled = false;
            IDictionary<string, CustomSetting> settings = infoBase.CustomSettings;
            if (settings.Count == 0) return;
            if (settings.Keys.Contains(CONST_LogEntitySettingName))
            {
                Guid identity = new Guid(settings[CONST_LogEntitySettingName].Value);
                this.LogEntity = Entity.Select(identity);
                if (this.LogEntity != null)
                {
                    this.LogEntityName = this.LogEntity.FullName;
                    foreach (Property property in this.LogEntity.Properties)
                    {
                        this.LogEntityProperties.Add(property.Name);
                    }
                }
                this.IsLogEntityEnabled = true;
            }
            if (settings.Keys.Contains(CONST_LogPropertySettingName))
            {
                _LogEntityProperty = settings[CONST_LogPropertySettingName].Value;
            }
        }
        public Entity LogEntity { get; private set; }
        public string LogEntityName { get; private set; }
        public ICommand UpdateTextBoxSourceCommand { get; private set; }
        private void OnUpdateTextBoxSource(object userControl)
        {
            TextBox textBox = userControl as TextBox;
            if (textBox == null) return;
            DependencyProperty property = TextBox.TextProperty;
            BindingExpression binding = BindingOperations.GetBindingExpression(textBox, property);
            if (binding == null) return;
            binding.UpdateSource();
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

        private string _Name = string.Empty;
        private string _Server = string.Empty;
        private string _Database = string.Empty;
        public string Name
        {
            get { return infoBase.Name; }
            set
            {
                try
                {
                    _Name = infoBase.Name;
                    infoBase.Name = value;
                    infoBase.Save();
                    OnPropertyChanged("Name");
                }
                catch (Exception ex)
                {
                    infoBase.Name = _Name;
                    _Name = string.Empty;
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                }
            }
        }
        public string Server
        {
            get { return infoBase.Server; }
            set
            {
                try
                {
                    _Server = infoBase.Server;
                    infoBase.Server = value;
                    infoBase.Save();
                    OnPropertyChanged("Server");
                }
                catch (Exception ex)
                {
                    infoBase.Server = _Server;
                    _Server = string.Empty;
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                }
            }
        }
        public string Database
        {
            get { return infoBase.Database; }
            set
            {
                try
                {
                    _Database = infoBase.Database;
                    infoBase.Database = value;
                    infoBase.Save();
                    OnPropertyChanged("Database");
                }
                catch (Exception ex)
                {
                    infoBase.Database = _Database;
                    _Database = string.Empty;
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                }
            }
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

        public InteractionRequest<Notification> SelectEntityPopupRequest { get; private set; }
        public ICommand SelectLogEntityCommand { get; private set; }
        public ICommand ClearLogEntityCommand { get; private set; }
        public bool IsLogEntityEnabled { get; private set; }
        public List<string> LogEntityProperties { get; private set; }
        private void OnSelectLogEntity()
        {
            Confirmation notification = new Confirmation() { Content = infoBase, Title = CONST_ModuleDialogsTitle };
            this.SelectEntityPopupRequest.Raise(notification, response =>
            {
                Confirmation confirmation = response as Confirmation;
                if (confirmation == null) return;
                if (!confirmation.Confirmed) return;
                this.SelectLogEntity(confirmation.Content as Entity);
            });
        }
        private void SelectLogEntity(Entity entity)
        {
            if (entity == null) return;
            if (this.LogEntity == entity) return;
            try
            {
                SaveCustomSetting(CONST_LogEntitySettingName, entity.Identity.ToString());
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                return;
            }
            this.LogEntity = entity;
            this.LogEntityName = this.LogEntity.FullName;
            this.OnPropertyChanged("LogEntityName");
            this.LogEntityProperties = new List<string>();
            foreach (Property property in this.LogEntity.Properties)
            {
                this.LogEntityProperties.Add(property.Name);
            }
            this.OnPropertyChanged("LogEntityProperties");
            if (this.LogEntityProperties.Count > 0)
            {
                this.LogEntityProperty = this.LogEntityProperties[0];
            }
            else
            {
                _LogEntityProperty = string.Empty;
            }
            this.OnPropertyChanged("LogEntityProperty");
            this.IsLogEntityEnabled = true;
            this.OnPropertyChanged("IsLogEntityEnabled");
        }
        private void OnClearLogEntity()
        {
            if (this.LogEntity == null) return;
            this.ClearLogEntity();
            this.OnPropertyChanged("LogEntityName");
            this.OnPropertyChanged("LogEntityProperties");
            this.OnPropertyChanged("LogEntityProperty");
            this.OnPropertyChanged("IsLogEntityEnabled");
        }
        private void ClearLogEntity()
        {
            if (this.LogEntity == null) return;
            try
            {
                KillCustomSetting(CONST_LogEntitySettingName);
                KillCustomSetting(CONST_LogPropertySettingName);
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                return;
            }
            this.LogEntity = null;
            this.LogEntityName = CONST_LogEntityNameTooltip;
            this.LogEntityProperties = new List<string>();
            _LogEntityProperty = string.Empty;
            this.IsLogEntityEnabled = false;
        }
        private string _LogEntityProperty = string.Empty;
        public string LogEntityProperty
        {
            get { return _LogEntityProperty; }
            private set
            {
                try
                {
                    SaveCustomSetting(CONST_LogPropertySettingName, value);
                    _LogEntityProperty = value;
                }
                catch (Exception ex)
                {
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                    return;
                }
            }
        }
        private void SaveCustomSetting(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            CustomSetting setting = null;
            if (infoBase.CustomSettings.TryGetValue(name, out setting))
            {
                setting.Value = value;
                setting.Save();
            }
            else
            {
                setting = CustomSetting.Create(infoBase, name, value);
            }
        }
        private void KillCustomSetting(string name)
        {
            CustomSetting setting = null;
            if (infoBase.CustomSettings.TryGetValue(name, out setting))
            {
                setting.Kill();
            }
        }
    }
}
