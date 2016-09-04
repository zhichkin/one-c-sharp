using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Zhichkin.Metadata.Controllers;
using System.Collections.ObjectModel;
using Zhichkin.Metadata.Commands;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.Services;

namespace Zhichkin.Metadata.ViewModels
{
    public class MetadataTreeViewModel : BindableBase
    {
        private readonly IMetadataService dataService;
        private readonly IEventAggregator eventAggregator;

        private ObservableCollection<InfoBase> infoBases = new ObservableCollection<InfoBase>();
        
        public MetadataTreeViewModel(IMetadataService dataService, IEventAggregator eventAggregator)
        {
            if (dataService == null) throw new ArgumentNullException("dataService");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");

            this.dataService = dataService;
            this.eventAggregator = eventAggregator;

            RefreshInfoBases();
        }
        private void RefreshInfoBases()
        {
            this.infoBases.Clear();
            foreach (InfoBase infoBase in dataService.GetInfoBases())
            {
                this.infoBases.Add(infoBase);
            }
        }
        public ObservableCollection<InfoBase> InfoBases
        {
            get
            {
                return infoBases;
            }
        }
        public object SelectedItem { get; private set; }
        public InfoBase CurrentInfoBase { get; set; }
        private void SetCurrentInfoBase(object model)
        {
            if (model is InfoBase)
            {
                CurrentInfoBase = (InfoBase)model;
                return;
            }
            if (model is Namespace)
            {
                CurrentInfoBase = GetInfoBase((Namespace)model);
                return;
            }
            if (model is Entity)
            {
                CurrentInfoBase = GetInfoBase(((Entity)model).Namespace);
                return;
            }
            if (model is Property)
            {
                CurrentInfoBase = GetInfoBase(((Property)model).Entity.Namespace);
                return;
            }
            if (model is Field)
            {
                CurrentInfoBase = GetInfoBase(((Field)model).Property.Entity.Namespace);
                return;
            }
            CurrentInfoBase = null;
        }
        private InfoBase GetInfoBase(Namespace _namespace)
        {
            if (_namespace == null) return null;

            Namespace currentNamespace = _namespace;
            while (currentNamespace.Owner.GetType() != typeof(InfoBase))
            {
                currentNamespace = (Namespace)currentNamespace.Owner;
            }
            return (InfoBase)currentNamespace.Owner;
        }
        public void SelectedItemChanged(object item)
        {
            SelectedItem = item;
            SetCurrentInfoBase(item);
            this.eventAggregator.GetEvent<MetadataTreeViewItemSelected>().Publish(item);
        }
    }
}
