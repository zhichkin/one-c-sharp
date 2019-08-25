using System;
using System.Linq;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.Services;

namespace Zhichkin.Metadata.UI
{
    public class SelectDataTypeDialogViewModel : BindableBase, IInteractionRequestAware
    {
        private InfoBase infoBase;
        private Confirmation notification;
        public SelectDataTypeDialogViewModel()
        {
            this.SelectCommand = new DelegateCommand(this.Confirm);
            this.CancelCommand = new DelegateCommand(this.Cancel);
        }
        public string Name { get { return (infoBase == null) ? string.Empty : infoBase.Name; } }
        private ObservableCollection<NamespaceViewModel> namespaces = new ObservableCollection<NamespaceViewModel>();
        public ObservableCollection<NamespaceViewModel> Namespaces
        {
            get
            {
                if (infoBase == null) return null;
                namespaces.Clear();

                if (infoBase.Name == "Metadata")
                {
                    foreach (Namespace ns in infoBase.Namespaces)
                    {
                        NamespaceViewModel facade = new NamespaceViewModel(ns);
                        foreach (Entity entity in ns.Entities)
                        {
                            facade.Entities.Add(entity);
                        }
                        namespaces.Add(facade);
                    }
                    return namespaces;
                }

                NamespaceViewModel typeSystem = new NamespaceViewModel(Namespace.TypeSystem);
                typeSystem.Entities.Add(Entity.Boolean);
                typeSystem.Entities.Add(Entity.Decimal);
                typeSystem.Entities.Add(Entity.Int32);
                typeSystem.Entities.Add(Entity.String);
                typeSystem.Entities.Add(Entity.DateTime);
                namespaces.Add(typeSystem);

                MetadataService metadata = new MetadataService();
                InfoBase system = metadata.GetSystemInfoBase();
                foreach (Namespace ns in system.Namespaces)
                {
                    if (ns.Name == "TypeSystem") continue;

                    NamespaceViewModel facade = new NamespaceViewModel(ns);
                    foreach (Entity entity in ns.Entities)
                    {
                        facade.Entities.Add(entity);
                    }
                    namespaces.Add(facade);
                }

                foreach (Namespace ns in infoBase.Namespaces)
                {
                    if (ns.Name == "Перечисление" ||
                        ns.Name == "Справочник" ||
                        ns.Name == "Документ")
                    {
                        NamespaceViewModel facade = new NamespaceViewModel(ns);
                        foreach (Entity entity in ns.Entities)
                        {
                            facade.Entities.Add(entity);
                        }
                        namespaces.Add(facade);
                    }
                    else
                    {
                        NamespaceViewModel facade = new NamespaceViewModel(ns);
                        foreach (Entity entity in ns.Entities)
                        {
                            if (entity.Parent != null)
                            {
                                facade.Entities.Add(entity);
                            }
                        }
                        namespaces.Add(facade);
                    }
                }
                return namespaces;
            }
        }
        public object SelectedNode { set; get; }
        public ICommand SelectCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public void Confirm()
        {
            if (this.notification != null)
            {
                if (this.SelectedNode == null)
                {
                    this.notification.Confirmed = false;
                    this.notification.Content = null;
                }
                else
                {
                    this.notification.Confirmed = true;
                    this.notification.Content = this.SelectedNode;
                }
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
                infoBase = this.notification.Content as InfoBase;
                this.OnPropertyChanged("Namespaces");
                this.OnPropertyChanged("Name");
            }
        }
        public Action FinishInteraction { get; set; }
    }
}
