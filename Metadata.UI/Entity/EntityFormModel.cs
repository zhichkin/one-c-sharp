using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Transactions;
using System.Windows.Input;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.Services;
using Zhichkin.ORM;
using Zhichkin.Shell;

namespace Zhichkin.Metadata.UI
{
    public sealed class EntityFormModel : BindableBase, IInteractionRequestAware
    {
        private Entity model;
        private Confirmation notification;

        public EntityFormModel()
        {
            this.PropertyPopup = new InteractionRequest<Confirmation>();
            this.SelectDataTypeDialog = new InteractionRequest<Confirmation>();
            this.CreateNewPropertyCommand = new DelegateCommand(this.CreateNewProperty);
            this.SelectParentEntityCommand = new DelegateCommand(this.OpenParentEntitySelectionDialog);

            this.KillPropertyCommand = new DelegateCommand(this.KillProperty);
            this.EditPropertyCommand = new DelegateCommand(this.EditProperty);
            this.CreateNewTableCommand = new DelegateCommand(this.CreateNewTable);

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

        public void EnterKeyIsPressed(string propertyName, string propertyValue)
        {
            if (this.model == null) return;

            if (propertyName == "Name")
            {
                this.Name = propertyValue;
            }
            else if (propertyName == "Alias")
            {
                this.Alias = propertyValue;
            }
            else if (propertyName == "Code")
            {
                this.Code = int.Parse(propertyValue);
            }
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

                this.model = value.Content as Entity;
                if (this.model == null) throw new ArgumentNullException("model");

                this.Properties = new ObservableCollection<Property>(this.model.Properties);

                if (this.model.MainTable == null)
                {
                    this.TableFields = new ObservableCollection<Field>();
                }
                else
                {
                    this.TableFields = new ObservableCollection<Field>(this.model.MainTable.Fields.OrderBy(f => f.Property.Ordinal));
                }

                this.RefreshView();
            }
        }
        private void RefreshView()
        {
            this.OnPropertyChanged("FormTitle");
            this.OnPropertyChanged("CancelButtonTitle");
            this.OnPropertyChanged("ConfirmButtonTitle");
            this.OnPropertyChanged("IsCancelButtonVisible");
            this.OnPropertyChanged("IsConfirmButtonVisible");

            this.OnPropertyChanged("Name");
            this.OnPropertyChanged("Alias");
            this.OnPropertyChanged("Code");
            this.OnPropertyChanged("TypeCode");
            this.OnPropertyChanged("Owner");
            this.OnPropertyChanged("Parent");
            this.OnPropertyChanged("Namespace");
            this.OnPropertyChanged("Properties");

            this.OnPropertyChanged("MainTableName");
            this.OnPropertyChanged("TableFields");
            this.OnPropertyChanged("IsTableInfoVisible");
            this.OnPropertyChanged("IsCreateNewTableButtonVisible");
        }
        public string FormTitle
        {
            get
            {
                if (this.model == null) return string.Empty;

                if (this.model.State == PersistentState.New)
                {
                    return $"Сущность \"{this.model.Name}\" (создание)";
                }
                else if (this.model.State == PersistentState.Original)
                {
                    return $"Сущность \"{this.model.Name}\"";
                }
                else if (this.model.State == PersistentState.Changed)
                {
                    return $"Сущность \"{this.model.Name}\" (изменение)";
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

        public ObservableCollection<Field> TableFields { set; get; }
        public ObservableCollection<Property> Properties { set; get; }

        public Entity Owner { get { return this.model?.Owner; } }
        public Namespace Namespace { get { return this.model?.Namespace; } }
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
        public string Alias
        {
            get
            {
                if (this.model == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this.model.Alias;
                }
            }
            set
            {
                if (this.model == null) return;
                this.model.Alias = value;
                this.RefreshView();
            }
        }
        public int TypeCode
        {
            get
            {
                if (this.model == null)
                {
                    return default(int);
                }
                else
                {
                    return this.model.TypeCode;
                }
            }
        }
        public int Code
        {
            get
            {
                if (this.model == null)
                {
                    return default(int);
                }
                else
                {
                    return this.model.Code;
                }
            }
            set
            {
                if (this.model == null) return;
                this.model.Code = value;
                this.RefreshView();
            }
        }
        public Entity Parent
        {
            get
            {
                if (this.model == null)
                {
                    return null;
                }
                else
                {
                    return this.model.Parent;
                }
            }
            set
            {
                if (this.model == null) return;
                this.model.Parent = value;
                this.RefreshView();
            }
        }

        public InteractionRequest<Confirmation> SelectDataTypeDialog { private set; get; }

        public ICommand SelectParentEntityCommand { private set; get; }
        private void OpenParentEntitySelectionDialog()
        {
            if (this.model == null) return;

            Confirmation confirmation = new Confirmation()
            {
                Title = "Выбор родительской сущности",
                Content = this.model.InfoBase
            };
            this.SelectDataTypeDialog.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    Entity content = response.Content as Entity;
                    if (content != null)
                    {
                        this.Parent = content;
                    }
                }
            });
        }

        public object SelectedProperty { get; set; }
        public InteractionRequest<Confirmation> PropertyPopup { get; private set; }
        public ICommand EditPropertyCommand { private set; get; }
        public ICommand KillPropertyCommand { private set; get; }
        public ICommand CreateNewPropertyCommand { private set; get; }
        private void CreateNewProperty()
        {
            if (this.model == null) throw new InvalidOperationException("Entity is null!");
            if (this.model.State == PersistentState.New)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = "Сущность не записана!" });
                return;
            }

            Property property = new Property()
            {
                Entity = this.model,
                Purpose = PropertyPurpose.Property,
                Ordinal = this.model.Properties.Count,
                Name = $"NewProperty{this.model.Properties.Count}"
            };

            Confirmation confirmation = new Confirmation()
            {
                Title = "Z-Metadata",
                Content = property
            };
            this.PropertyPopup.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    Property content = response.Content as Property;
                    if (content != null)
                    {
                        this.model.Properties.Add(content);
                        this.Properties.Add(content);
                    }
                }
            });
        }
        private void EditProperty()
        {
            if (this.model == null) throw new InvalidOperationException("Entity is null!");
            if (this.model.State == PersistentState.New)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = "Сущность не записана!" });
                return;
            }

            Property property = this.SelectedProperty as Property;
            if (property == null)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = "Свойство не выбрано." });
                return;
            }

            Confirmation confirmation = new Confirmation()
            {
                Title = "Z-Metadata",
                Content = property
            };
            this.PropertyPopup.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    property.OnPropertyChanged("Relations");
                }
            });
        }
        private void KillProperty()
        {
            if (this.model == null) throw new InvalidOperationException("Entity is null!");
            if (this.model.State == PersistentState.New)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = "Сущность не записана!" });
                return;
            }

            Property property = this.SelectedProperty as Property;
            if (property == null)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = "Свойство не выбрано." });
                return;
            }

            bool cancel = true;
            Z.Confirm(new Confirmation
            {
                Title = "Z-Metadata",
                Content = $"Свойство \"{property.Name}\" и все его\nподчинённые объекты будут удалены.\n\nПродолжить ?"
            },
                c => { cancel = !c.Confirmed; }
            );

            if (cancel) return;

            try
            {
                IMetadataService dataService = new MetadataService();
                TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, options))
                {
                    dataService.Kill(property);
                    scope.Complete();
                }
                this.Properties.Remove(property);
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = Z.GetErrorText(ex) });
            }
        }

        public string MainTableName
        {
            get
            {
                if (this.model == null) return string.Empty;
                if (this.model.MainTable == null) return string.Empty;
                if (string.IsNullOrWhiteSpace(this.model.MainTable.Schema))
                {
                    return $"[{this.model.MainTable.Name}]";
                }
                else
                {
                    return $"[{this.model.MainTable.Schema}].[{this.model.MainTable.Name}]";
                }
            }
        }
        public bool IsTableInfoVisible
        {
            get
            {
                if (this.model == null) return false;
                return (this.model.MainTable != null);
            }
        }
        public bool IsCreateNewTableButtonVisible
        {
            get
            {
                if (this.model == null) return true;
                return (this.model.MainTable == null);
            }
        }
        public ICommand CreateNewTableCommand { private set; get; }
        private void CreateNewTable()
        {
            if (this.model == null) throw new InvalidOperationException("Entity is null!");
            if (this.model.State == PersistentState.New)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = "Сущность не записана!" });
                return;
            }

            Table table = new Table()
            {
                Entity = this.model,
                Purpose = TablePurpose.Main,
                Schema = "dbo",
                Name = $"NewTable_{this.model.Name}"
            };

            Confirmation confirmation = new Confirmation()
            {
                Title = "Z-Metadata",
                Content = table
            };
            this.PropertyPopup.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    Table content = response.Content as Table;
                    if (content != null)
                    {
                        this.OnPropertyChanged("MainTableName");
                        this.OnPropertyChanged("TableFields");
                        this.OnPropertyChanged("IsTableInfoVisible");
                        this.OnPropertyChanged("IsCreateNewTableButtonVisible");
                    }
                }
            });
        }

    }
}
