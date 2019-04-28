using Microsoft.Practices.Prism.Commands;
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

        public ParameterReferenceViewModel GetParameterReferenceViewModel(HermesViewModel parent)
        {
            return new ParameterReferenceViewModel(parent, (ParameterExpression)this.Model);
        }
    }
}
