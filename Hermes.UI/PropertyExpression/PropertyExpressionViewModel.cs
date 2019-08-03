using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Zhichkin.Hermes.Model;
using Zhichkin.Shell;

namespace Zhichkin.Hermes.UI
{
    public class PropertyExpressionViewModel : HermesViewModel
    {
        private UserControl _ExpressionView;
        // TODO: move this dialog to SelectExpressionViewModel to create it once
        private InteractionRequest<Confirmation> _PropertySelectionDialog;

        public PropertyExpressionViewModel(HermesViewModel parent, PropertyExpression model) : base(parent, model)
        {
            this.RemovePropertyCommand = new DelegateCommand(this.RemoveProperty);
            this.OpenPropertySelectionDialogCommand = new DelegateCommand(this.OpenPropertySelectionDialog);

            this.IntializeViewModel(model);
        }
        private void IntializeViewModel(PropertyExpression model)
        {
            if (model.Expression == null)
            {
                return;
            }
            if (model.Expression is PropertyReference)
            {
                SelectStatementViewModel parent = this.Parent as SelectStatementViewModel;
                if (parent != null)
                {
                    PropertyReference property = (PropertyReference)model.Expression;
                    TableExpressionViewModel tableVM = parent.Tables.Where(t => t.Alias == property.Table.Alias).FirstOrDefault();
                    PropertyReferenceViewModel propertyVM = tableVM.Properties.Where(p => p.Name == property.Name).FirstOrDefault();
                    if (propertyVM != null)
                    {
                        this.OnExpressionSelected(propertyVM);
                    }
                }
            }
        }

        public UserControl ExpressionView
        {
            get { return _ExpressionView; }
            set
            {
                _ExpressionView = value;
                this.OnPropertyChanged("ExpressionView");
            }
        }
        public HermesViewModel Expression { get; set; }
        public string Alias
        {
            get
            {
                if (this.Model == null) return string.Empty;
                return ((PropertyExpression)this.Model).Alias;
            }
            set
            {
                if (this.Model == null) return;
                ((PropertyExpression)this.Model).Alias = value;
                OnPropertyChanged("Alias");
            }
        }
        public ICommand OpenPropertySelectionDialogCommand { get; private set; }
        public InteractionRequest<Confirmation> PropertySelectionDialog
        {
            get
            {
                // This is necessary because this view model can be bound to several views
                // while rebuilding view tree. Each time it is bound to the new view it will
                // register new trigger with this view model's InteractionRequest.Raised event
                _PropertySelectionDialog = new InteractionRequest<Confirmation>();
                return _PropertySelectionDialog;
            }
        }
        private void OpenPropertySelectionDialog()
        {
            Confirmation confirmation = new Confirmation() { Title = "Select property", Content = this };
            _PropertySelectionDialog.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    OnExpressionSelected((HermesViewModel)response.Content);
                }
            });
        }
        private void OnExpressionSelected(HermesViewModel selectedExpression)
        {
            PropertyExpression model = (PropertyExpression)this.Model;

            if (selectedExpression == null)
            {
                model.Expression = null;
                this.Expression = null;
                this.ExpressionView = null;
                return;
            }

            model.Expression = selectedExpression.Model;
            this.Expression = selectedExpression;
            this.Expression.Parent = this;

            if (selectedExpression is PropertyReferenceViewModel)
            {
                model.Expression.Consumer = model;
                this.Alias = ((PropertyReferenceViewModel)this.Expression).Name; // this sets model's property Alias as well
                this.ExpressionView = new PropertyReferenceView((PropertyReferenceViewModel)this.Expression);
            }
            else if (selectedExpression is ParameterReferenceViewModel)
            {
                this.Alias = ((ParameterReferenceViewModel)this.Expression).Name; // this sets model's property Alias as well
                this.ExpressionView = new ParameterReferenceView((ParameterReferenceViewModel)this.Expression);
            }
        }

        public ICommand RemovePropertyCommand { get; private set; }
        private void RemoveProperty()
        {
            SelectStatementViewModel parent = this.Parent as SelectStatementViewModel;
            if (parent == null) return;
            parent.RemoveProperty(this);
        }
    }
}
