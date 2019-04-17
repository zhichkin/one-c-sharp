using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System.Windows.Input;
using Zhichkin.Hermes.Model;
using Zhichkin.Shell;

namespace Zhichkin.Hermes.UI
{
    public class PropertyExpressionViewModel : HermesViewModel
    {
        private bool _IsAliasVisible;

        public PropertyExpressionViewModel(HermesViewModel parent, PropertyExpression model) : base(parent, model)
        {
            _IsAliasVisible = parent is SelectStatementViewModel; // SELECT clause check
            this.PropertySelectionDialog = new InteractionRequest<Confirmation>();
            this.OpenPropertySelectionDialogCommand = new DelegateCommand(this.OpenPropertySelectionDialog);
        }
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
        public bool IsAliasVisible
        {
            get { return _IsAliasVisible; }
            set
            {
                _IsAliasVisible = value;
                this.OnPropertyChanged("IsAliasVisible");
            }
        }

        public ICommand OpenPropertySelectionDialogCommand { get; private set; }
        public InteractionRequest<Confirmation> PropertySelectionDialog { get; private set; }
        private void OpenPropertySelectionDialog()
        {
            Confirmation confirmation = new Confirmation() { Title = "Select property", Content = this };
            this.PropertySelectionDialog.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    Z.Notify(new Notification { Title = "Hermes", Content = response.Content.ToString() });
                }
            });
        }
    }
}
