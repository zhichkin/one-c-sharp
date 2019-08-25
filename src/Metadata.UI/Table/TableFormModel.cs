using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using System;
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
    public sealed class TableFormModel : BindableBase, IInteractionRequestAware
    {
        private Table model;
        private Confirmation notification;
        private SQLBuilder sqlService = new SQLBuilder();

        public TableFormModel()
        {
            this.ConfirmCommand = new DelegateCommand(this.Confirm);
            this.CancelCommand = new DelegateCommand(this.Cancel);

            this.FieldPopup = new InteractionRequest<Confirmation>();
            this.KillFieldCommand = new DelegateCommand(this.KillField);
            this.EditFieldCommand = new DelegateCommand(this.EditField);
            this.CreateNewFieldCommand = new DelegateCommand(this.CreateNewField);

            this.DropTableCommand = new DelegateCommand(this.DropTable);
            this.CreateTableCommand = new DelegateCommand(this.CreateTable);
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
            else if (propertyName == "Schema")
            {
                this.Schema = propertyValue;
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

                this.model = value.Content as Table;
                if (this.model == null) throw new ArgumentNullException("model");

                this.TableFields = new ObservableCollection<Field>(this.model.Fields.OrderBy(f => f.Property?.Ordinal ?? 0));

                _IsDropTableButtonVisible = sqlService.TableExists(this.model);
                _IsCreateTableButtonVisible = !_IsDropTableButtonVisible;

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

            this.OnPropertyChanged("IsDropTableButtonVisible");
            this.OnPropertyChanged("IsCreateTableButtonVisible");

            this.OnPropertyChanged("Name");
            this.OnPropertyChanged("Schema");
            this.OnPropertyChanged("Entity");

            this.OnPropertyChanged("TableFields");
        }
        public string FormTitle
        {
            get
            {
                if (this.model == null) return string.Empty;

                if (this.model.State == PersistentState.New)
                {
                    return $"Таблица \"{this.model.Name}\" (создание)";
                }
                else if (this.model.State == PersistentState.Original)
                {
                    return $"Таблица \"{this.model.Name}\"";
                }
                else if (this.model.State == PersistentState.Changed)
                {
                    return $"Таблица \"{this.model.Name}\" (изменение)";
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

        public Entity Entity { get { return this.model?.Entity; } }
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
        public string Schema
        {
            get
            {
                if (this.model == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this.model.Schema;
                }
            }
            set
            {
                if (this.model == null) return;
                this.model.Schema = value;
                this.RefreshView();
            }
        }

        public ObservableCollection<Field> TableFields { set; get; }
        public object SelectedField { get; set; }
        public InteractionRequest<Confirmation> FieldPopup { get; private set; }
        public ICommand EditFieldCommand { private set; get; }
        public ICommand KillFieldCommand { private set; get; }
        public ICommand CreateNewFieldCommand { private set; get; }
        private void CreateNewField()
        {
            if (this.model == null) throw new InvalidOperationException("Entity is null!");
            if (this.model.State == PersistentState.New)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = "Сущность не записана!" });
                return;
            }

            Field field = new Field()
            {
                Table = this.model,
                Purpose = FieldPurpose.Value,
                Name = $"NewField{this.model.Fields.Count}"
            };

            Confirmation confirmation = new Confirmation()
            {
                Title = "Z-Metadata",
                Content = field
            };
            this.FieldPopup.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    Field content = response.Content as Field;
                    if (content != null)
                    {
                        this.model.Fields.Add(content);
                        this.TableFields.Add(content);
                    }
                }
            });
        }
        private void EditField()
        {
            if (this.model == null) throw new InvalidOperationException("Model is null!");
            if (this.model.State == PersistentState.New)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = "Сущность не записана!" });
                return;
            }

            Field field = this.SelectedField as Field;
            if (field == null)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = "Поле не выбрано." });
                return;
            }

            Confirmation confirmation = new Confirmation()
            {
                Title = "Z-Metadata",
                Content = field
            };
            this.FieldPopup.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    field.OnPropertyChanged("TableFields");
                }
            });
        }
        private void KillField()
        {
            if (this.model == null) throw new InvalidOperationException("Model is null!");
            if (this.model.State == PersistentState.New)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = "Сущность не записана!" });
                return;
            }

            Field field = this.SelectedField as Field;
            if (field == null)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = "Поле не выбрано." });
                return;
            }

            bool cancel = true;
            Z.Confirm(new Confirmation
            {
                Title = "Z-Metadata",
                Content = $"Поле \"{field.Name}\" и все его\nподчинённые объекты будут удалены.\n\nПродолжить ?"
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
                    dataService.Kill(field);
                    scope.Complete();
                }
                this.TableFields.Remove(field);
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = Z.GetErrorText(ex) });
            }
        }

        private bool _IsDropTableButtonVisible = false;
        private bool _IsCreateTableButtonVisible = false;
        public bool IsDropTableButtonVisible { get { return _IsDropTableButtonVisible; } }
        public bool IsCreateTableButtonVisible { get { return _IsCreateTableButtonVisible; } }
        public ICommand DropTableCommand { get; private set; }
        public ICommand CreateTableCommand { get; private set; }
        private void DropTable()
        {
            if (this.model == null) throw new InvalidOperationException("Model is null!");
            if (this.model.State == PersistentState.New)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = "Свойства таблицы не записаны!" });
                return;
            }

            bool cancel = true;
            Z.Confirm(new Confirmation
                {
                    Title = "Z-Metadata",
                    Content = $"Таблица \"{this.model.Name}\" будет удалена.\n\nПродолжить ?"
                },
                c => { cancel = !c.Confirmed; }
            );
            if (cancel) return;

            try
            {
                sqlService.DropTable(this.model);
                _IsDropTableButtonVisible = sqlService.TableExists(this.model);
                _IsCreateTableButtonVisible = !_IsDropTableButtonVisible;
                this.OnPropertyChanged("IsDropTableButtonVisible");
                this.OnPropertyChanged("IsCreateTableButtonVisible");
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = Z.GetErrorText(ex) });
            }
        }
        private void CreateTable()
        {
            try
            {
                sqlService.CreateTable(this.model);
                _IsDropTableButtonVisible = sqlService.TableExists(this.model);
                _IsCreateTableButtonVisible = !_IsDropTableButtonVisible;
                this.OnPropertyChanged("IsDropTableButtonVisible");
                this.OnPropertyChanged("IsCreateTableButtonVisible");
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = Z.GetErrorText(ex) });
            }
        }
    }
}
