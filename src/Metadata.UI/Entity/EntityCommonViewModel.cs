using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.Services;
using Zhichkin.ORM;
using Zhichkin.Shell;

namespace Zhichkin.Metadata.UI
{
    public sealed class EntityCommonViewModel : BindableBase, IInteractionRequestAware
    {
        private Entity model;
        private Confirmation notification;
        private UIElement _View;

        public EntityCommonViewModel()
        {
            this.ConfirmCommand = new DelegateCommand(this.Confirm);
            this.CancelCommand = new DelegateCommand(this.Cancel);

            this.PropertyPopup = new InteractionRequest<Confirmation>();
            this.EditPropertyCommand = new DelegateCommand(this.EditProperty);
            this.KillPropertyCommand = new DelegateCommand(this.KillProperty);
            this.CreateNewPropertyCommand = new DelegateCommand<string>(this.CreateNewProperty);

            this.TablePopup = new InteractionRequest<Confirmation>();
            this.EditTableCommand = new DelegateCommand(this.EditTable);
            this.CreateNewTableCommand = new DelegateCommand(this.CreateNewTable);
        }
        public EntityCommonViewModel(Entity entity) : this()
        {
            if (entity == null) throw new ArgumentNullException("entity");
            this.model = entity;
            this.InitializeViewModel();
        }
        public UIElement View
        {
            get { return _View; }
            set
            {
                _View = value;
                this.OnPropertyChanged("View");
            }
        }
        public ICommand ConfirmCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public Action FinishInteraction { get; set; }
        public void Confirm()
        {
            try
            {
                this.SaveModel();
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = Z.GetErrorText(ex) });
                return;
            }

            if (this.notification != null)
            {
                this.notification.Confirmed = true;
                this.FinishInteraction();
            }
        }
        public void Cancel()
        {
            if (this.notification != null)
            {
                this.notification.Confirmed = false;
            }
            this.FinishInteraction();
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
        
        public INotification Notification
        {
            get
            {
                return this.notification;
            }
            set
            {
                this.notification = value as Confirmation;
                if (this.notification == null) throw new ArgumentException("notification");

                this.model = value.Content as Entity;
                if (this.model == null) throw new ArgumentException("model");

                this.InitializeViewModel();

                this.OnPropertyChanged("FormTitle");
                this.OnPropertyChanged("CancelButtonTitle");
                this.OnPropertyChanged("ConfirmButtonTitle");
                this.OnPropertyChanged("IsCancelButtonVisible");
                this.OnPropertyChanged("IsConfirmButtonVisible");

                this.OnPropertyChanged("TableName");
                this.OnPropertyChanged("TableFields");
                this.OnPropertyChanged("OwnProperties");
                this.OnPropertyChanged("AbstractProperties");
                this.OnPropertyChanged("IsTableInfoVisible");
                this.OnPropertyChanged("IsCreateNewTableButtonVisible");
            }
        }
        private void InitializeViewModel()
        {
            this.OwnProperties = new ObservableCollection<Property>(
                this.model.Properties
                .Where(p => !p.IsAbstract)
                .OrderBy(p => p.Ordinal));

            this.AbstractProperties = new ObservableCollection<Property>(
                this.model.Properties
                .Where(p => p.IsAbstract)
                .OrderBy(p => p.Ordinal));

            if (this.model.MainTable != null)
            {
                this.TableFields = new ObservableCollection<Field>(this.model.MainTable.Fields.OrderBy(f => f.Property?.Ordinal ?? 0));
            }

            List<Entity> inheritanceChain = GetInheritanceChain(this.model);
            UIElement grid = BuildPropertiesGrid(inheritanceChain);
            this.View = grid;
        }
        
        public ObservableCollection<Field> TableFields { get; private set; } = new ObservableCollection<Field>();
        public ObservableCollection<Property> OwnProperties { get; private set; } = new ObservableCollection<Property>();
        public ObservableCollection<Property> AbstractProperties { get; private set; } = new ObservableCollection<Property>();

        public object SelectedProperty { get; set; }
        public InteractionRequest<Confirmation> PropertyPopup { get; private set; }
        public ICommand EditPropertyCommand { private set; get; }
        public ICommand KillPropertyCommand { private set; get; }
        public ICommand CreateNewPropertyCommand { private set; get; }
        private void CreateNewProperty(string parameter)
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
                Name = $"NewProperty{this.model.Properties.Count}",
                IsAbstract = (parameter == "Abstract")
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
                        if(content.IsAbstract)
                        {
                            this.AbstractProperties.Add(content);
                        }
                        else
                        {
                            this.OwnProperties.Add(content);
                        }
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
                dataService.Kill(property);
                if (property.IsAbstract)
                {
                    this.AbstractProperties.Remove(property);
                }
                else
                {
                    this.OwnProperties.Remove(property);
                }
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = Z.GetErrorText(ex) });
            }
        }

        public string TableName
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
        public ICommand EditTableCommand { get; private set; }
        public ICommand CreateNewTableCommand { get; private set; }
        public InteractionRequest<Confirmation> TablePopup { get; private set; }
        private void EditTable()
        {
            if (this.model == null) throw new InvalidOperationException("Entity is null!");
            if (this.model.State == PersistentState.New)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = "Сущность не записана!" });
                return;
            }

            Table table = this.model.MainTable;
            if (table == null)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = "Таблица не выбрана." });
                return;
            }

            Confirmation confirmation = new Confirmation()
            {
                Title = "Z-Metadata",
                Content = table
            };
            this.TablePopup.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    this.OnPropertyChanged("TableName");
                    this.OnPropertyChanged("TableFields");
                }
            });
        }
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
            this.TablePopup.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    Table content = response.Content as Table;
                    if (content != null)
                    {
                        this.OnPropertyChanged("TableName");
                        this.OnPropertyChanged("TableFields");
                        this.OnPropertyChanged("IsTableInfoVisible");
                        this.OnPropertyChanged("IsCreateNewTableButtonVisible");
                    }
                }
            });
        }

        private void SaveModel()
        {
            //TODO: build SQL commands programmatically
        }

        private List<Entity> GetInheritanceChain(Entity child)
        {
            if (child == null) throw new ArgumentNullException("child");

            List<Entity> chain = new List<Entity>() { child };

            Entity parent = child.Parent;
            while (parent != null)
            {
                chain.Insert(0, parent);
                parent = parent.Parent;
            }

            return chain;
        }
        private UIElement BuildPropertiesGrid(List<Entity> inheritanceChain)
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            int rowIndex = 0;
            foreach (Entity item in inheritanceChain)
            {
                UIElement ui = BuildEntityView(item);

                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                Grid.SetRow(ui, rowIndex);
                Grid.SetColumn(ui, 0);
                grid.Children.Add(ui);

                rowIndex++;
            }

            return grid;
        }
        private UIElement BuildEntityView(Entity entity)
        {
            Expander expander = new Expander()
            {
                IsExpanded = false,
                ExpandDirection = ExpandDirection.Down,
                Header = new TextBlock()
                {
                    Text = entity.Name + ((entity == this.model) ? " (собственные свойства)" : string.Empty),
                    Margin = new Thickness(4),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Content = new ScrollViewer()
                {
                    MaxHeight = 250,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
                }
            };

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 100, Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 100, Width = GridLength.Auto });

            int rowIndex = 0;
            foreach (Property property in entity.Properties
                .Where(p => !p.IsAbstract)
                .OrderBy(p => p.Ordinal))
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                TextBlock textBlock = new TextBlock()
                {
                    Text = $"{property.Name}:",
                    Margin = new Thickness(5),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Grid.SetRow(textBlock, rowIndex);
                Grid.SetColumn(textBlock, 0);
                grid.Children.Add(textBlock);

                object value = null;
                if (property.Relations.Count > 0)
                {
                    value = Entity.GetDefaultValue(property.Relations[0].Entity);
                }

                MetadataPropertyViewModel propertyViewModel = new MetadataPropertyViewModel(this, property, value);
                MetadataPropertyView propertyView = new MetadataPropertyView(propertyViewModel);
                Grid.SetRow(propertyView, rowIndex);
                Grid.SetColumn(propertyView, 1);
                grid.Children.Add(propertyView);

                rowIndex++;
            }

            ScrollViewer sv = (ScrollViewer)expander.Content;
            sv.Content = grid;
            return expander;
        }

        private void LoadModel()
        {
            //TODO: load model from SQL
        }
    }
}
