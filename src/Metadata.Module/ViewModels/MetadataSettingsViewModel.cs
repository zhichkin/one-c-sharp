using System;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System.Configuration;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using Zhichkin.Metadata.Model;
using System.Data;
using System.Data.SqlClient;
using Zhichkin.Shell;

namespace Zhichkin.Metadata.ViewModels
{
    public sealed class MetadataSettingsViewModel : BindableBase
    {
        private readonly string moduleName = MetadataPersistentContext.Current.Name;
        private const string CONST_ModuleDialogsTitle = "Z-Metadata";
        private const string CONST_MetadataCatalogSettingName = "MetadataCatalog";

        private string _MetadataConnectionString = string.Empty;
        private string _MetadataCatalogSetting = string.Empty;

        public MetadataSettingsViewModel()
        {
            this.UpdateTextBoxSourceCommand = new DelegateCommand<object>(this.OnUpdateTextBoxSource);
            this.CheckConnectionCommand = new DelegateCommand(this.OnCheckConnection);
            _MetadataConnectionString = ConfigurationManager.ConnectionStrings[moduleName].ConnectionString;
            _MetadataCatalogSetting = ConfigurationManager.AppSettings[CONST_MetadataCatalogSettingName];
        }
        public ICommand CheckConnectionCommand { get; private set; }
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
                
        public string MetadataConnectionString
        {
            get { return _MetadataConnectionString; }
            set
            {
                try
                {
                    UpdateMetadataConnectionString(value);
                    _MetadataConnectionString = value;
                    OnPropertyChanged("MetadataConnectionString");
                }
                catch (Exception ex)
                {
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                }
            }
        }
        private void UpdateMetadataConnectionString(string connectionString)
        {
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

            Z.Notify(new Notification
            {
                Title = CONST_ModuleDialogsTitle,
                Content = string.Format("Настройка строки соединения для модуля \"{0}\" выполнена.", moduleName)
            });
        }
        private void OnCheckConnection()
        {
            string resultMessage = string.Empty;
            SqlConnection connection = new SqlConnection(_MetadataConnectionString);
            try
            {
                connection.Open();
                if (connection.State == ConnectionState.Open)
                {
                    resultMessage = "Соединение открыто успешно.";
                }
                else
                {
                    resultMessage = string.Format("Соединение получило статус: \"{0}\".", connection.State.ToString());
                }
            }
            catch (Exception ex)
            {
                resultMessage = GetErrorText(ex);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
                connection.Dispose();
            }
            Z.Notify(new Notification
            {
                Title = CONST_ModuleDialogsTitle, Content = resultMessage
            });
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

        public string MetadataCatalogSetting
        {
            get { return _MetadataCatalogSetting; }
            set
            {
                try
                {
                    UpdateMetadataCatalogSetting(value);
                    _MetadataCatalogSetting = value;
                    OnPropertyChanged("MetadataCatalogSetting");
                }
                catch (Exception ex)
                {
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                }
            }
        }
        private void UpdateMetadataCatalogSetting(string catalog)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            AppSettingsSection settings = config.AppSettings;
            KeyValueConfigurationElement setting = settings.Settings[CONST_MetadataCatalogSettingName];
            if (setting == null)
            {
                settings.Settings.Add(CONST_MetadataCatalogSettingName, catalog);
            }
            else
            {
                setting.Value = catalog;
            }
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            Z.Notify(new Notification
            {
                Title = CONST_ModuleDialogsTitle,
                Content = string.Format("Настройка служебного каталога для модуля \"{0}\" выполнена.", moduleName)
            });
        }
    }
}