using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System.Windows;
using System.Windows.Controls;
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
            this.RemoveParameterCommand = new DelegateCommand(this.RemoveParameter);
            this.OpenTypeSelectionDialogCommand = new DelegateCommand(this.OpenTypeSelectionDialog);
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
                SetValueView();
                this.OnPropertyChanged("Value");
            }
        }

        public string TypeButtonContent
        {
            get
            {
                return (this.Type == null) ? "T" : "...";
            }
        }
        public string TypeButtonToolTip
        {
            get
            {
                return (this.Type == null) ? "Select type of the value" : this.Type.FullName;
            }
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

        private void SetValueView()
        {
            ParameterExpression model = this.Model as ParameterExpression;
            if (model == null) this.ValueView = null;
            if (this.Value == null) this.ValueView = null;

            TextBlock view = new TextBlock();
            view.Text = this.Value.ToString();
            this.ValueView = view;
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

            this.Type= entity;
        }

        public ParameterReferenceViewModel GetParameterReferenceViewModel(HermesViewModel parent)
        {
            return new ParameterReferenceViewModel(parent, (ParameterExpression)this.Model);
        }
    }
}
