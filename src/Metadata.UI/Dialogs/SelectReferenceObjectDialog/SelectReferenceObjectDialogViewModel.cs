using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Metadata.UI
{
    public class SelectReferenceObjectDialogViewModel : BindableBase, IInteractionRequestAware
    {
        private Entity metadata;
        private Confirmation notification;
        private ISqlCommandBuilder commander;
        private List<FilterParameter> _filter;

        public SelectReferenceObjectDialogViewModel()
        {
            this.SelectCommand = new DelegateCommand(this.Confirm);
            this.CancelCommand = new DelegateCommand(this.Cancel);
            this.FilterCommand = new DelegateCommand(this.Filter);
            this.SelectParametersDialog = new InteractionRequest<Confirmation>();
        }
        public ICommand FilterCommand { get; private set; }
        public InteractionRequest<Confirmation> SelectParametersDialog { get; private set; }
        public Entity Metadata { get { return metadata; } }
        public string Name { get { return (metadata == null) ? string.Empty : metadata.FullName; } }
        public List<dynamic> Items
        {
            get
            {
                if (metadata == null) return null;
                return this.GetItems();
            }
        }
        # region " Basic functions "
        public dynamic SelectedItem { set; get; }
        public ICommand SelectCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public void Confirm()
        {
            if (this.notification != null)
            {
                this.notification.Confirmed = true;
                this.notification.Content = this.GetReturnValue();
            }
            this.FinishInteraction();
        }
        public void Cancel()
        {
            if (this.notification != null)
            {
                this.notification.Confirmed = false;
                this.notification.Content = null;
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
                if (this.notification == null) return;
                metadata = this.notification.Content as Entity;
                commander = new SqlCommandBuilder(metadata);
                commander.Build();
                this.OnPropertyChanged("Name");
                this.OnPropertyChanged("Items");
            }
        }
        public Action FinishInteraction { get; set; }
        #endregion

        private ReferenceProxy GetReturnValue()
        {
            if (this.SelectedItem == null) return null;
            return new ReferenceProxy(metadata, this.SelectedItem.Ссылка.Identity);
        }
        private List<dynamic> GetItems()
        {
            return commander.Select();
        }
        public async Task<List<dynamic>> GetItemsAsync(int pageNumber, int pageSize)
        {
            return commander.Select(pageNumber, pageSize, _filter);
        }
        public int GetItemsCount()
        {
            return commander.Count();
        }

        private void Filter()
        {
            Confirmation confirmation = new Confirmation()
            {
                Title = "Параметры отбора",
                Content = metadata
            };
            this.SelectParametersDialog.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    _filter = response.Content as List<FilterParameter>;
                    this.OnPropertyChanged("Filter");
                }
            });
        }
    }
}
