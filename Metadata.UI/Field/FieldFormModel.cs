using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Zhichkin.Metadata.Model;
using Zhichkin.ORM;

namespace Zhichkin.Metadata.UI
{
    public sealed class FieldFormModel : BindableBase, IInteractionRequestAware
    {
        private Field model;
        private Confirmation notification;

        public FieldFormModel()
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

        public void EnterKeyIsPressed(string propertyName, string propertyValue)
        {
            if (this.model == null) return;

            if (propertyName == "Name")
            {
                this.Name = propertyValue;
            }
            else if (propertyName == "TypeName")
            {
                this.TypeName = propertyValue;
            }
            else if (propertyName == "Length")
            {
                this.Length = int.Parse(propertyValue);
            }
            else if (propertyName == "Precision")
            {
                this.Precision = int.Parse(propertyValue);
            }
            else if (propertyName == "Scale")
            {
                this.Scale = int.Parse(propertyValue);
            }
            else if (propertyName == "KeyOrdinal")
            {
                this.KeyOrdinal = byte.Parse(propertyValue);
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

                this.model = value.Content as Field;
                if (this.model == null) throw new ArgumentNullException("model");

                this.PropertiesItemsSource = this.Table.Entity.Properties.ToList();

                this.PurposesItemsSource = new List<FieldPurpose>()
                {
                    FieldPurpose.Value,
                    FieldPurpose.Object,
                    FieldPurpose.TypeCode,
                    FieldPurpose.Locator,
                    FieldPurpose.String,
                    FieldPurpose.Number,
                    FieldPurpose.Boolean,
                    FieldPurpose.DateTime,
                    FieldPurpose.Binary
                };
                
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
            this.OnPropertyChanged("Table");

            this.OnPropertyChanged("TypeName");
            this.OnPropertyChanged("Length");
            this.OnPropertyChanged("Precision");
            this.OnPropertyChanged("Scale");
            this.OnPropertyChanged("IsNullable");
            this.OnPropertyChanged("IsPrimaryKey");
            this.OnPropertyChanged("KeyOrdinal");

            this.OnPropertyChanged("SelectedPropertyItem");
            this.OnPropertyChanged("PropertiesItemsSource");

            this.OnPropertyChanged("SelectedPurposeItem");
            this.OnPropertyChanged("PurposesItemsSource");
        }
        public string FormTitle
        {
            get
            {
                if (this.model == null) return string.Empty;

                if (this.model.State == PersistentState.New)
                {
                    return $"Поле \"{this.model.Name}\" (создание)";
                }
                else if (this.model.State == PersistentState.Original)
                {
                    return $"Поле \"{this.model.Name}\"";
                }
                else if (this.model.State == PersistentState.Changed)
                {
                    return $"Поле \"{this.model.Name}\" (изменение)";
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

        public Table Table { get { return this.model?.Table; } }
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
        public bool IsNullable
        {
            get
            {
                if (this.model == null)
                {
                    return false;
                }
                else
                {
                    return this.model.IsNullable;
                }
            }
            set
            {
                if (this.model == null) return;
                this.model.IsNullable = value;
                this.RefreshView();
            }
        }
        public bool IsPrimaryKey
        {
            get
            {
                if (this.model == null)
                {
                    return false;
                }
                else
                {
                    return this.model.IsPrimaryKey;
                }
            }
            set
            {
                if (this.model == null) return;
                this.model.IsPrimaryKey = value;
                this.RefreshView();
            }
        }
        public byte KeyOrdinal
        {
            get
            {
                if (this.model == null)
                {
                    return default(byte);
                }
                else
                {
                    return this.model.KeyOrdinal;
                }
            }
            set
            {
                if (this.model == null) return;
                this.model.KeyOrdinal = value;
                this.RefreshView();
            }
        }
        public string TypeName
        {
            get
            {
                if (this.model == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this.model.TypeName;
                }
            }
            set
            {
                if (this.model == null) return;
                this.model.TypeName = value;
                this.RefreshView();
            }
        }
        public int Length
        {
            get
            {
                if (this.model == null)
                {
                    return default(int);
                }
                else
                {
                    return this.model.Length;
                }
            }
            set
            {
                if (this.model == null) return;
                this.model.Length = value;
                this.RefreshView();
            }
        }
        public int Precision
        {
            get
            {
                if (this.model == null)
                {
                    return default(int);
                }
                else
                {
                    return this.model.Precision;
                }
            }
            set
            {
                if (this.model == null) return;
                this.model.Precision = value;
                this.RefreshView();
            }
        }
        public int Scale
        {
            get
            {
                if (this.model == null)
                {
                    return default(int);
                }
                else
                {
                    return this.model.Scale;
                }
            }
            set
            {
                if (this.model == null) return;
                this.model.Scale = value;
                this.RefreshView();
            }
        }

        public Property SelectedPropertyItem
        {
            get
            {
                if (this.model == null) return null;
                return this.model.Property;
            }
            set
            {
                if (value == null) return;
                if (this.model == null) return;
                this.model.Property = value;
                this.RefreshView();
            }
        }
        public List<Property> PropertiesItemsSource { private set; get; }

        public FieldPurpose SelectedPurposeItem
        {
            get
            {
                if (this.model == null) return FieldPurpose.Value;
                return this.model.Purpose;
            }
            set
            {
                if (this.model == null) return;
                this.model.Purpose = value;
                this.RefreshView();
            }
        }
        public List<FieldPurpose> PurposesItemsSource { private set; get; }
    }
}
