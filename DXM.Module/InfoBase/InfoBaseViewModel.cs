using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Commands;
using Zhichkin.Metadata.Model;
using Zhichkin.Shell;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Data;
using System.Data.SqlClient;

namespace Zhichkin.DXM.Module
{
    public class InfoBaseViewModel : BindableBase
    {
        private readonly InfoBase _infoBase;
        private readonly IUnityContainer _container;

        private string _Name = string.Empty;
        private PublicationsListView _PublicationsListView;
        
        public InfoBaseViewModel(InfoBase model, IUnityContainer container)
        {
            if (model == null) throw new ArgumentNullException("model");
            _infoBase = model;
            _container = container;
            InitializeViewModel();
        }
        public void InitializeViewModel()
        {
            this.UpdateTextBoxSourceCommand = new DelegateCommand<object>(this.OnUpdateTextBoxSource);
            this.CheckConnectionCommand = new DelegateCommand(this.OnCheckConnection);
            this.DatabaseSettingsPopupRequest = new InteractionRequest<DatabaseSettingsNotification>();
            this.ShowDatabaseSettingsPopupCommand = new DelegateCommand(this.ShowDatabaseSettingsPopup);
            _PublicationsListView = (PublicationsListView)_container.Resolve(
                typeof(PublicationsListView),
                new ParameterOverride("publisher", _infoBase)
                    .OnType(typeof(PublicationsListViewModel)));
        }
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
        public InteractionRequest<DatabaseSettingsNotification> DatabaseSettingsPopupRequest { get; private set; }
        public ICommand ShowDatabaseSettingsPopupCommand { get; private set; }
        public ICommand CheckConnectionCommand { get; private set; }
        public PublicationsListView PublicationsListView { get { return _PublicationsListView; } }
        public string Name
        {
            get { return _infoBase.Name; }
            set
            {
                try
                {
                    _Name = _infoBase.Name;
                    _infoBase.Name = value;
                    _infoBase.Save();
                    OnPropertyChanged("Name");
                }
                catch (Exception ex)
                {
                    _infoBase.Name = _Name;
                    _Name = string.Empty;
                    Z.Notify(new Notification { Title = Utilities.PopupDialogsTitle, Content = ExceptionsHandling.GetErrorText(ex) });
                }
            }
        }
        private void ShowDatabaseSettingsPopup()
        {
            bool cancel = false;
            DatabaseSettingsNotification notification = new DatabaseSettingsNotification()
            {
                Title = Utilities.PopupDialogsTitle,
                Server = _infoBase.Server,
                Database = _infoBase.Database,
                UserName = _infoBase.UserName,
                Password = _infoBase.Password
            };
            this.DatabaseSettingsPopupRequest.Raise(notification, response =>
            {
                if (!response.Confirmed)
                {
                    cancel = true;
                }
                else
                {
                    _infoBase.Server = response.Server;
                    _infoBase.Database = response.Database;
                    _infoBase.UserName = response.UserName;
                    _infoBase.Password = response.Password;
                }
            });
        }
        private void OnCheckConnection()
        {
            SqlConnectionStringBuilder helper = new SqlConnectionStringBuilder()
            {
                DataSource = _infoBase.Server,
                InitialCatalog = _infoBase.Database,
                IntegratedSecurity = string.IsNullOrWhiteSpace(_infoBase.UserName),
                UserID = string.IsNullOrWhiteSpace(_infoBase.UserName) ? string.Empty:_infoBase.UserName,
                Password = string.IsNullOrWhiteSpace(_infoBase.Password) ? string.Empty : _infoBase.Password
            };

            string resultMessage = string.Empty;
            SqlConnection connection = new SqlConnection(helper.ConnectionString);
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
                resultMessage = ExceptionsHandling.GetErrorText(ex);
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
                Title = Utilities.PopupDialogsTitle,
                Content = resultMessage
            });
        }
    }
}
