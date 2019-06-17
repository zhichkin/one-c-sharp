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
            }
        }
        private void InitializeViewModel()
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 100, Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 100, Width = GridLength.Auto });

            Dictionary<Entity, IList<Property>> lookup = new Dictionary<Entity, IList<Property>>();
            Entity parent = this.model;
            while (parent != null)
            {
                if (parent.Properties.Count > 0)
                {
                    lookup.Add(parent, parent.Properties);
                }
                parent = parent.Parent;
            }

            int rowIndex = 0;
            foreach(KeyValuePair<Entity, IList<Property>> item in lookup)
            {
                foreach (Property property in item.Value)
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
                    
                    MetadataPropertyViewModel propertyViewModel = new MetadataPropertyViewModel(this, property);
                    MetadataPropertyView propertyView = new MetadataPropertyView(propertyViewModel);
                    Grid.SetRow(propertyView, rowIndex);
                    Grid.SetColumn(propertyView, 1);
                    grid.Children.Add(propertyView);

                    rowIndex++;
                }
            }

            this.View = grid;
        }
        private void SaveModel()
        {
            //TODO: build SQL commands programmatically
        }
        public ObservableCollection<MetadataPropertyViewModel> Properties { get; private set; }
    }
}
