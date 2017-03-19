using System;
using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;
using Zhichkin.Metadata.Model;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Zhichkin.Shell;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace Zhichkin.Metadata.Module
{
    public class DifferenceTreeViewModel : BindableBase
    {
        private const string CONST_ModuleDialogsTitle = "Z-Metadata";
        private ObservableCollection<IDifferenceObject> _infoBases = new ObservableCollection<IDifferenceObject>();
        
        public DifferenceTreeViewModel(IDifferenceObject model)
        {
            _infoBases.Add(model);
            this.ApplyChangesCommand = new DelegateCommand(this.OnApplyChanges);
        }
        public ObservableCollection<IDifferenceObject> InfoBases { get { return _infoBases; } }
        public ICommand ApplyChangesCommand { get; private set; }

        private void OnApplyChanges()
        {
            try
            {
                Z.Confirm(new Confirmation { Title = CONST_ModuleDialogsTitle, Content = "Применить изменения ?" },
                    c => { if (c.Confirmed) this.ApplyChanges(); });
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = ExceptionsHandling.GetErrorText(ex) });
            }
        }
        private void ApplyChanges()
        {
            IDifferenceService service = new DifferenceService();
            service.Apply(_infoBases[0]);
            _infoBases.Clear();
        }
    }
}
