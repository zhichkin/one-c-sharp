using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using System;
using System.Configuration;
using System.Data.SqlClient;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.Views;
using Zhichkin.Shell;

namespace Zhichkin.Metadata.UI
{
    public sealed class SetupWizardViewModel : BindableBase
    {
        private const string CONST_ModuleDialogsTitle = "Z-Metadata";
        private const string CONST_DefaultDatabaseName = "one-c-sharp";
        private const string CONST_ViewModelDialogsTitle = "1C# database setup";
        private readonly string moduleName = MetadataPersistentContext.Current.Name;
        private MetadataPersistentContext dataContext = (MetadataPersistentContext)MetadataPersistentContext.Current;

        private readonly IUnityContainer unity;
        private readonly IRegionManager manager;
        private readonly IRegionViewRegistry regions;

        public SetupWizardViewModel(IUnityContainer container, IRegionViewRegistry registry, IRegionManager manager) : base()
        {
            this.unity = container;
            this.manager = manager;
            this.regions = registry;
            InitializeViewModel();
        }
        private void InitializeViewModel()
        {
            string connectionString = dataContext.ConnectionString;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            this.ServerName = builder.DataSource ?? string.Empty;
            this.DatabaseName = builder.InitialCatalog ?? string.Empty;
            this.UseWindowsAuthentication = builder.IntegratedSecurity;
            this.UserName = builder.UserID ?? string.Empty;
        }

        public string ServerName { get; set; }
        private string _DatabaseName = string.Empty;
        public string DatabaseName
        {
            get { return _DatabaseName; }
            set
            {
                _DatabaseName = value;
                this.OnPropertyChanged(nameof(DatabaseName));
            }
        }
        public bool UseWindowsAuthentication { get; set; }
        public string UserName { get; set; }

        public void SetupDatabase(string password)
        {
            SaveConnectionStringSettings(password);
            dataContext.RefreshConnectionString();

            if (!dataContext.CheckServerConnection())
            {
                Z.Notify(new Notification() { Title = CONST_ViewModelDialogsTitle, Content = "Error connecting server. Try again, please." });
                return;
            }

            if (dataContext.CheckDatabaseConnection() && dataContext.CheckTables())
            {
                GoToStartupView();
                return;
            }

            try
            {
                dataContext.SetupDatabase();
                SetDefaultDatabaseName(password);
                dataContext.RefreshConnectionString();
                dataContext.CreateMetaModel();
                Z.Notify(new Notification() { Title = CONST_ViewModelDialogsTitle, Content = "1C# database has been setup successfully =)" });
                GoToStartupView();
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification() { Title = CONST_ViewModelDialogsTitle, Content = Z.GetErrorText(ex) });
            }
        }
        private void GoToStartupView()
        {
            Z.ClearRightRegion(this.manager);
            regions.RegisterViewWithRegion(RegionNames.TopRegion, () => this.unity.Resolve<MetadataMainMenu>());
            regions.RegisterViewWithRegion(RegionNames.LeftRegion, () => this.unity.Resolve<MetadataTreeView>());
        }
        private void SaveConnectionStringSettings(string password)
        {
            SqlConnectionStringBuilder helper = new SqlConnectionStringBuilder()
            {
                DataSource = this.ServerName,
                InitialCatalog = this.DatabaseName
            };
            if (this.UseWindowsAuthentication)
            {
                helper.IntegratedSecurity = true;
            }
            else
            {
                helper.UserID = this.UserName;
                helper.Password = password;
            }
            string connectionString = helper.ToString();

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConnectionStringSettingsCollection settings = config.ConnectionStrings.ConnectionStrings;
            ConnectionStringSettings setting = settings[moduleName];
            if (setting == null)
            {
                settings.Add(new ConnectionStringSettings(moduleName, connectionString, "System.Data.SqlClient"));
            }
            else
            {
                setting.ConnectionString = connectionString;
            }
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("connectionStrings");
        }
        private void SetDefaultDatabaseName(string password)
        {
            this.DatabaseName = CONST_DefaultDatabaseName;
            SqlConnectionStringBuilder helper = new SqlConnectionStringBuilder()
            {
                DataSource = this.ServerName,
                InitialCatalog = this.DatabaseName
            };
            if (this.UseWindowsAuthentication)
            {
                helper.IntegratedSecurity = true;
            }
            else
            {
                helper.UserID = this.UserName;
                helper.Password = password;
            }
            string connectionString = helper.ToString();

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConnectionStringSettingsCollection settings = config.ConnectionStrings.ConnectionStrings;
            ConnectionStringSettings setting = settings[moduleName];
            if (setting == null)
            {
                settings.Add(new ConnectionStringSettings(moduleName, connectionString, "System.Data.SqlClient"));
            }
            else
            {
                setting.ConnectionString = connectionString;
            }
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("connectionStrings");
        }
    }
}
