using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System.Windows.Input;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class PropertyReferenceViewModel: HermesViewModel
    {
        private TableExpressionViewModel _Table;
        public PropertyReferenceViewModel(HermesViewModel parent, TableExpressionViewModel table, PropertyReference model) : base(parent, model)
        {
            this.Table = table;
            this.PropertySelectionDialog = new InteractionRequest<Confirmation>();
            this.OpenPropertySelectionDialogCommand = new DelegateCommand(this.OpenPropertySelectionDialog);
        }
        public TableExpressionViewModel Table
        {
            get { return _Table; }
            set
            {
                _Table = value;
                this.OnPropertyChanged("Table");
            }
        }
        public PropertyReferenceViewModel Property
        {
            get { return this; }
            set
            {
                this.Model = value?.Model;
            }
        }
        public string Name
        {
            get
            {
                if (this.Model == null) return string.Empty;
                return ((PropertyReference)this.Model).Name;
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
                    PropertyReferenceViewModel result = response.Content as PropertyReferenceViewModel;
                    if (result == null) return;
                    this.Table = result.Table;
                    this.OnPropertyChanged("Name"); // ?
                }
            });
        }
    }
}
