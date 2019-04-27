using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
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
            this.OpenPropertySelectionDialogCommand = new DelegateCommand(this.OpenPropertySelectionDialog);
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
            if (selectedExpression == null)
            {
                this.Expression = null;
                this.ExpressionView = null;
            }

            PropertyReferenceViewModel viewModel = selectedExpression as PropertyReferenceViewModel;
            if (selectedExpression == null) return;

            PropertyExpression model = this.Model as PropertyExpression;
            model.Expression = selectedExpression.Model;
            model.Expression.Consumer = model;

            this.Expression = selectedExpression;
            this.Expression.Parent = this;
            this.Alias = viewModel.Name; // this sets model's property Alias as well
            this.ExpressionView = new PropertyReferenceView((PropertyReferenceViewModel)this.Expression);
        }
    }
}
