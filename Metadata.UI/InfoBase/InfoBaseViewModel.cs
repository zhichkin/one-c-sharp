using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Windows.Input;
using Zhichkin.Metadata.Model;
using Zhichkin.ORM;

namespace Zhichkin.Metadata.UI
{
    public sealed class InfoBaseViewModel : BindableBase, IInteractionRequestAware
    {
        private InfoBase model;
        private Confirmation notification;

        public InfoBaseViewModel()
        {
            this.ConfirmCommand = new DelegateCommand(this.Confirm);
            this.CancelCommand = new DelegateCommand(this.Cancel);
        }
        public ICommand ConfirmCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public Action FinishInteraction { get; set; }
        public void Confirm()
        {
            this.model.Save();

            if (this.notification != null)
            {
                this.notification.Confirmed = true;
            }
            this.FinishInteraction();
        }
        public void Cancel()
        {
            if (this.model.State == PersistentState.Changed)
            {
                this.model.Load();
                this.model.OnPropertyChanged("Name");
            }

            if (this.notification != null)
            {
                this.notification.Confirmed = false;
            }
            this.FinishInteraction();
        }

        public INotification Notification
        {
            get
            {
                return this.notification;
            }
            set
            {
                this.notification = value as Confirmation;
                if (this.notification == null) throw new IndexOutOfRangeException("notification");

                this.model = value.Content as InfoBase;
                if (this.model == null) throw new ArgumentNullException("model");

                this.RefreshView();
            }
        }
        private void RefreshView()
        {
            this.OnPropertyChanged("Name");
            this.OnPropertyChanged("Server");
            this.OnPropertyChanged("Database");
            this.OnPropertyChanged("UserName");
            this.OnPropertyChanged("Password");

            this.OnPropertyChanged("InfoBaseViewTitle");
            this.OnPropertyChanged("CancelButtonTitle");
            this.OnPropertyChanged("ConfirmButtonTitle");
            this.OnPropertyChanged("IsCancelButtonVisible");
            this.OnPropertyChanged("IsConfirmButtonVisible");
        }
        public string Name
        {
            get
            {
                if (this.model == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this.model.Name;
                }
            }
            set
            {
                if (this.model == null) return;
                this.model.Name = value;
                this.RefreshView();
            }
        }
        public string Server
        {
            get
            {
                if (this.model == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this.model.Server;
                }
            }
            set
            {
                if (this.model == null) return;
                this.model.Server = value;
                this.RefreshView();
            }
        }
        public string Database
        {
            get
            {
                if (this.model == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this.model.Database;
                }
            }
            set
            {
                if (this.model == null) return;
                this.model.Database = value;
                this.RefreshView();
            }
        }
        public string UserName
        {
            get
            {
                if (this.model == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this.model.UserName;
                }
            }
            set
            {
                if (this.model == null) return;
                this.model.UserName = value;
                this.RefreshView();
            }
        }
        public string Password
        {
            get
            {
                if (this.model == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this.model.Password;
                }
            }
            set
            {
                if (this.model == null) return;
                this.model.Password = value;
                this.RefreshView();
            }
        }
        public void EnterKeyIsPressed(string propertyName, string propertyValue)
        {
            if (this.model == null) return;

            if (propertyName == "Name")
            {
                this.Name = propertyValue;
            }
            else if (propertyName == "Server")
            {
                this.Server = propertyValue;
            }
            else if (propertyName == "Database")
            {
                this.Database = propertyValue;
            }
            else if (propertyName == "UserName")
            {
                this.UserName = propertyValue;
            }
            else if (propertyName == "Password")
            {
                this.Password = propertyValue;
            }
        }

        public string InfoBaseViewTitle
        {
            get
            {
                if (this.model == null) return string.Empty;

                if (this.model.State == PersistentState.New)
                {
                    return $"{this.model.Name} (создание)";
                }
                else if (this.model.State == PersistentState.Changed)
                {
                    return $"{this.model.Name} (изменение)"; ;
                }
                return this.model.Name;
            }
        }
        public bool IsConfirmButtonVisible
        {
            get
            {
                if (this.model == null) return false;
                return (this.model.State == PersistentState.New
                    || this.model.State == PersistentState.Changed);
            }
        }
        public string ConfirmButtonTitle
        {
            get
            {
                if (this.model == null) return string.Empty;

                if (this.model.State == PersistentState.New)
                {
                    return "Сохранить";
                }
                else if (this.model.State == PersistentState.Changed)
                {
                    return "Изменить";
                }
                return "Записать";
            }
        }
        public bool IsCancelButtonVisible
        {
            get
            {
                if (this.model == null) return false;
                return (this.model.State == PersistentState.New
                    || this.model.State == PersistentState.Changed);
            }
        }
        public string CancelButtonTitle
        {
            get
            {
                if (this.model == null) return string.Empty;
                return "Отменить";
            }
        }
    }
}
