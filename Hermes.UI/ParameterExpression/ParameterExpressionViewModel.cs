using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.UI
{
    public class ParameterExpressionViewModel : HermesViewModel
    {
        private UIElement _ValueView;

        public ParameterExpressionViewModel(HermesViewModel parent, ParameterExpression model) : base(parent, model)
        {
            this.ClearParameterCommand = new DelegateCommand(this.ClearParameter);
            this.RemoveParameterCommand = new DelegateCommand(this.RemoveParameter);
            this.OpenTypeSelectionDialogCommand = new DelegateCommand(this.OpenTypeSelectionDialog);
            this.OpenReferenceObjectDialogCommand = new DelegateCommand(this.OpenReferenceObjectDialog);
        }
        public string Name
        {
            get
            {
                if (this.Model == null) return string.Empty;
                return ((ParameterExpression)this.Model).Name;
            }
            set
            {
                if (this.Model == null) return;
                ((ParameterExpression)this.Model).Name = value;
                this.OnPropertyChanged("Name");
            }
        }
        public Entity Type
        {
            get
            {
                if (this.Model == null) return null;
                return ((ParameterExpression)this.Model).Type;
            }
            set
            {
                if (this.Model == null) return;
                ((ParameterExpression)this.Model).Type = value;
                this.OnPropertyChanged("Type");
                this.OnPropertyChanged("TypeButtonContent");
                this.OnPropertyChanged("TypeButtonToolTip");
            }
        }
        public object Value
        {
            get
            {
                if (this.Model == null) return null;
                return ((ParameterExpression)this.Model).Value;
            }
            set
            {
                if (this.Model == null) return;
                ((ParameterExpression)this.Model).Value = value;
                // TODO: convert numeric values to Int32 or Decimal
                this.OnPropertyChanged("Value");
            }
        }

        public string TypeButtonContent
        {
            get { return "T"; }
        }
        public string TypeButtonToolTip
        {
            get { return "Select type of the value"; }
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

        private void SetupValueView()
        {
            ParameterExpression model = this.Model as ParameterExpression;
            if (model == null)
            {
                this.ValueView = null;
                return;
            }

            if (this.Type == null)
            {
                this.ValueView = null;
                return;
            }

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
            else if (this.Value == null)
            {
                TextBlock view = new TextBlock();
                view.Text = this.Type.FullName;
                this.ValueView = view;
            }
            else
            {
                ReferenceProxy proxy = this.Value as ReferenceProxy;
                TextBlock view = new TextBlock();
                view.Text = proxy.ToString();
                view.ToolTip = proxy.Type.FullName;
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

        public ICommand ClearParameterCommand { get; private set; }
        private void ClearParameter()
        {
            this.Type = null;
            this.Value = null;
            this.ValueView = null;
            this.OnPropertyChanged("IsClearButtonVisible");
            this.OnPropertyChanged("IsReferenceObjectSelectionEnabled");
        }
        public bool IsClearButtonVisible
        {
            get { return this.Type != null; }
        }

        public ICommand RemoveParameterCommand { get; private set; }
        private void RemoveParameter()
        {
            QueryExpressionViewModel parent = this.Parent as QueryExpressionViewModel;
            if (parent == null) return;
            parent.RemoveParameterCommand.Execute(this.Name);
        }

        public ICommand OpenTypeSelectionDialogCommand { get; private set; }
        private void OpenTypeSelectionDialog()
        {
            QueryExpressionViewModel parent = this.Parent as QueryExpressionViewModel;
            if (parent == null) return;

            Confirmation confirmation = new Confirmation() { Title = "Select data type", Content = this };
            parent.TypeSelectionDialog.Raise(confirmation, response =>
            {
                if (response.Confirmed) { OnTypeSelected(response.Content); }
            });
        }
        private void OnTypeSelected(object selectedType)
        {
            if (selectedType == null) return;
            MetadataNodeViewModel vm = selectedType as MetadataNodeViewModel;
            if (vm == null) return;
            MetadataNode model = vm.Model;
            if (model == null) return;
            Entity entity = model.Metadata as Entity;
            if (entity == null) return;

            this.Type = entity;
            this.Value = null;

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
            QueryExpressionViewModel parent = this.Parent as QueryExpressionViewModel;
            if (parent == null) return;

            Entity entity = this.Type;
            Confirmation confirmation = new Confirmation() { Title = entity.Name, Content = entity };
            parent.ReferenceObjectSelectionDialog.Raise(confirmation, response =>
            {
                if (response.Confirmed) { OnReferenceObjectSelected(response.Content); }
            });
        }
        private void OnReferenceObjectSelected(object selectedReference)
        {
            ReferenceProxy proxy = selectedReference as ReferenceProxy;
            this.Value = proxy;
            this.SetupValueView();
        }

        public ParameterReferenceViewModel GetParameterReferenceViewModel(HermesViewModel parent)
        {
            return new ParameterReferenceViewModel(parent, (ParameterExpression)this.Model);
        }
    }
}
