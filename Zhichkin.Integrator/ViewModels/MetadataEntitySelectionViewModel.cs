﻿using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Metadata.ViewModels
{
    public class MetadataEntitySelectionViewModel : BindableBase, IInteractionRequestAware
    {
        private InfoBase infoBase;
        private Confirmation notification;
        public MetadataEntitySelectionViewModel()
        {
            this.SelectCommand = new DelegateCommand(this.Confirm);
            this.CancelCommand = new DelegateCommand(this.Cancel);
        }
        private ObservableCollection<Namespace> namespaces = new ObservableCollection<Namespace>();
        public ObservableCollection<Namespace> Namespaces
        {
            get
            {
                if (infoBase == null) return null;
                namespaces.Clear();
                foreach (Namespace ns in infoBase.Namespaces)
                {
                    namespaces.Add(ns);
                }
                return namespaces;
            }
        }
        public Entity SelectedEntity { set; get; }
        public ICommand SelectCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public void Confirm()
        {
            if (this.notification != null)
            {
                this.notification.Confirmed = true;
                this.notification.Content = this.SelectedEntity;
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
            }
        }
        public Action FinishInteraction { get; set; }
    }
}