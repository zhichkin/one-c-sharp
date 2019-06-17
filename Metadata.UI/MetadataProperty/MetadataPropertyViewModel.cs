using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Metadata.UI
{
    public sealed class MetadataPropertyViewModel : BindableBase
    {
        private Property model;
        private UIElement _ValueView;
        private readonly EntityCommonViewModel parent;

        public MetadataPropertyViewModel(EntityCommonViewModel parent, Property model)
        {
            this.model = model ?? throw new ArgumentNullException("model");
            this.parent = parent ?? throw new ArgumentNullException("parent");

            //TODO: get real value of the property from database !
            //this.Value = typeof(Entity).GetProperty(model.Name).GetValue(model.Entity);
            
            if (model.Relations.Count == 0)
            {
                this.Type = null;
                this.Value = null;
            }
            else if (model.Relations.Count == 1)
            {
                this.Type = model.Relations[0].Entity;
                this.Value = Entity.GetDefaultValue(this.Type);
            }
            else
            {
                this.Type = Entity.Object;
                this.Value = null;
            }
            OnTypeSelected(this.Type);

            this.ClearValueCommand = new DelegateCommand(this.ClearValue);
            this.OpenReferenceObjectDialogCommand = new DelegateCommand(this.OpenReferenceObjectDialog);
        }
        public UIElement ValueView
        {
            get { return _ValueView; }
            set
            {
                _ValueView = value;
                this.OnPropertyChanged("ValueView");
            }
        }

        public string Name
        {
            get
            {
                return this.model.Name;
            }
        }
        public Entity Type { get; set; }
        private object _Value;
        public object Value
        {
            get
            {
                return _Value;
            }
            set
            {
                if (this.Type == Entity.Int32)
                {
                    _Value = Int32.Parse(value.ToString());
                }
                else if (this.Type == Entity.Decimal)
                {
                    _Value = decimal.Parse(value.ToString());
                }
                else
                {
                    _Value = value;
                }
            }
        }

        private void SetupValueView()
        {
            if (this.Type == Entity.Int32
                || this.Type == Entity.Decimal
                || this.Type == Entity.String)
            {
                SetupTextBoxView();
            }
            else if (this.Type == Entity.Boolean)
            {
                SetupCheckBoxView();
            }
            else if (this.Type == Entity.DateTime)
            {
                SetupDatePickerView();
            }
            else
            {
                TextBlock view = new TextBlock();
                view.Text = this.Type.FullName;
                this.ValueView = view;
            }
        }
        private void SetupTextBoxView()
        {
            TextBox view = new TextBox()
            {
                Text = string.Empty,
                MinWidth = 100,
                Height = 24,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            Binding binding = new Binding("Value")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            };
            if (this.Type == Entity.Int32 || this.Type == Entity.Decimal)
            {
                binding.StringFormat = "{}{0:G}";
            }
            view.SetBinding(TextBox.TextProperty, binding);
            this.ValueView = view;
        }
        private void SetupCheckBoxView()
        {
            CheckBox view = new CheckBox() { IsChecked = false };
            Binding binding = new Binding("Value")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            };
            view.SetBinding(CheckBox.IsCheckedProperty, binding);
            this.ValueView = view;
        }
        private void SetupDatePickerView()
        {
            this.Value = DateTime.Now;
            DatePicker view = new DatePicker();
            Binding binding = new Binding("Value")
            {
                Source = this,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            view.SetBinding(DatePicker.SelectedDateProperty, binding);
            this.ValueView = view;
        }

        public bool IsClearButtonVisible
        {
            get { return this.Type != null; }
        }
        public ICommand ClearValueCommand { get; private set; }
        private void ClearValue()
        {
            this.Type = null;
            this.Value = null;
            this.ValueView = null;
            this.OnPropertyChanged("IsClearButtonVisible");
            this.OnPropertyChanged("IsReferenceObjectSelectionEnabled");
        }
        
        private void OnTypeSelected(Entity type)
        {
            //this.Type = type;
            //this.Value = null;

            this.SetupValueView();
            this.OnPropertyChanged("IsClearButtonVisible");
            this.OnPropertyChanged("IsReferenceObjectSelectionEnabled");
        }

        public bool IsReferenceObjectSelectionEnabled
        {
            get
            {
                return (this.Type != null
                    && this.Type != Entity.Int32
                    && this.Type != Entity.String
                    && this.Type != Entity.Decimal
                    && this.Type != Entity.Boolean
                    && this.Type != Entity.DateTime);
            }
        }
        public ICommand OpenReferenceObjectDialogCommand { get; private set; }
        private void OpenReferenceObjectDialog()
        {
            //QueryExpressionViewModel parent = this.Parent as QueryExpressionViewModel;
            //if (parent == null) return;

            //Entity entity = this.Type;
            //Confirmation confirmation = new Confirmation() { Title = entity.Name, Content = entity };
            //parent.ReferenceObjectSelectionDialog.Raise(confirmation, response =>
            //{
            //    if (response.Confirmed) { OnReferenceObjectSelected(response.Content); }
            //});
        }
        private void OnReferenceObjectSelected(object selectedReference)
        {
            ReferenceProxy proxy = selectedReference as ReferenceProxy;
            this.Value = proxy;
            this.SetupValueView();
        }
    }
}
