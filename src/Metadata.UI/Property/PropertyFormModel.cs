using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Zhichkin.Metadata.Model;
using Zhichkin.ORM;
using Zhichkin.Shell;

namespace Zhichkin.Metadata.UI
{
    public sealed class PropertyFormModel : BindableBase, IInteractionRequestAware
    {
        private Property model;
        private Confirmation notification;

        public PropertyFormModel()
        {
            this.SelectDataTypeDialog = new InteractionRequest<Confirmation>();
            this.OpenDataTypeSelectionDialogCommand = new DelegateCommand(this.OpenDataTypeSelectionDialog);
            
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

                this.model = value.Content as Property;
                this.Relations = new ObservableCollection<Relation>(this.model.Relations);
                if (this.model == null) throw new ArgumentNullException("model");

                this.RefreshView();
            }
        }
        private void RefreshView()
        {
            this.OnPropertyChanged("Name");
            this.OnPropertyChanged("Entity");
            this.OnPropertyChanged("IsAbstract");
            this.OnPropertyChanged("IsReadOnly");
            this.OnPropertyChanged("IsPrimaryKey");
            this.OnPropertyChanged("Relations");
            this.OnPropertyChanged("PurposeSelectedItem");

            this.OnPropertyChanged("FormTitle");
            this.OnPropertyChanged("CancelButtonTitle");
            this.OnPropertyChanged("ConfirmButtonTitle");
            this.OnPropertyChanged("IsCancelButtonVisible");
            this.OnPropertyChanged("IsConfirmButtonVisible");
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
        public bool IsAbstract
        {
            get
            {
                if (this.model == null)
                {
                    return false;
                }
                else
                {
                    return this.model.IsAbstract;
                }
            }
            set
            {
                if (this.model == null) return;
                this.model.IsAbstract = value;
                this.RefreshView();
            }
        }
        public bool IsReadOnly
        {
            get
            {
                if (this.model == null)
                {
                    return false;
                }
                else
                {
                    return this.model.IsReadOnly;
                }
            }
            set
            {
                if (this.model == null) return;
                this.model.IsReadOnly = value;
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

        private string _PurposeSelectedItem = "Свойство";
        public string PurposeSelectedItem
        {
            set
            {
                if (value == null) return;
                if (this.model == null) return;

                _PurposeSelectedItem = value;
                
                if (_PurposeSelectedItem == "Метаданные")
                {
                    this.model.Purpose = PropertyPurpose.System;
                }
                else if (_PurposeSelectedItem == "Свойство")
                {
                    this.model.Purpose = PropertyPurpose.Property;
                }
                else if (_PurposeSelectedItem == "Измерение")
                {
                    this.model.Purpose = PropertyPurpose.Dimension;
                }
                else if (_PurposeSelectedItem == "Ресурс")
                {
                    this.model.Purpose = PropertyPurpose.Measure;
                }
                else if (_PurposeSelectedItem == "Иерархия")
                {
                    this.model.Purpose = PropertyPurpose.Hierarchy;
                }
                this.RefreshView();
            }
            get
            {
                if (this.model == null)
                {
                    _PurposeSelectedItem = "Свойство";
                }
                else if (this.model.Purpose == PropertyPurpose.System)
                {
                    _PurposeSelectedItem = "Метаданные";
                }
                else if (this.model.Purpose == PropertyPurpose.Property)
                {
                    _PurposeSelectedItem = "Свойство";
                }
                else if (this.model.Purpose == PropertyPurpose.Dimension)
                {
                    _PurposeSelectedItem = "Измерение";
                }
                else if (this.model.Purpose == PropertyPurpose.Measure)
                {
                    _PurposeSelectedItem = "Ресурс";
                }
                else if (this.model.Purpose == PropertyPurpose.Hierarchy)
                {
                    _PurposeSelectedItem = "Иерархия";
                }
                return _PurposeSelectedItem;
            }
        }
        private List<string> _PurposesSelectionList = new List<string>()
        {
            "Метаданные", "Свойство", "Измерение", "Ресурс", "Иерархия"
        };
        public List<string> PurposesSelectionList
        {
            get
            {
                return _PurposesSelectionList;
            }
        }

        public void EnterKeyIsPressed(string propertyName, string propertyValue)
        {
            if (this.model == null) return;

            if (propertyName == "Name")
            {
                this.Name = propertyValue;
            }
        }

        public string FormTitle
        {
            get
            {
                if (this.model == null) return string.Empty;

                if (this.model.State == PersistentState.New)
                {
                    return $"Свойство \"{this.model.Name}\" (создание)";
                }
                else if (this.model.State == PersistentState.Original)
                {
                    return $"Свойство \"{this.model.Name}\"";
                }
                else if (this.model.State == PersistentState.Changed)
                {
                    return $"Свойство \"{this.model.Name}\" (изменение)";
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

        private ObservableCollection<Relation> _Relations = new ObservableCollection<Relation>();
        public ObservableCollection<Relation> Relations { set { _Relations = value; } get { return _Relations; } }
        public ICommand OpenDataTypeSelectionDialogCommand { private set; get; }
        public InteractionRequest<Confirmation> SelectDataTypeDialog { private set; get; }
        private void OpenDataTypeSelectionDialog()
        {
            if (this.model == null) return;
            if (this.model.State == PersistentState.New)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = "Свойство не записано!" });
                return;
            }

            Confirmation confirmation = new Confirmation()
            {
                Title = "Выберите тип значения",
                Content = this.model.Entity.InfoBase
            };
            this.SelectDataTypeDialog.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    Entity content = response.Content as Entity;
                    if (content != null)
                    {
                        Relation relation = new Relation()
                        {
                            Entity = content,
                            Property = this.model
                        };
                        try
                        {
                            relation.Save();
                            this.model.Relations.Add(relation);
                            this.Relations.Add(relation);
                        }
                        catch (Exception ex)
                        {
                            Z.Notify(new Notification { Title = "Z-Metadata", Content = Z.GetErrorText(ex) });
                        }
                    }
                }
            });
        }
    }
}
