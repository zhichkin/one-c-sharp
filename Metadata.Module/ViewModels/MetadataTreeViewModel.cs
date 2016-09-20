using System;
using System.Windows.Input;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;
using System.Collections.ObjectModel;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.Services;

namespace Zhichkin.Metadata.ViewModels
{
    public class MetadataTreeViewModel : BindableBase
    {
        private readonly IMetadataService dataService;
        private readonly IEventAggregator eventAggregator;

        private ObservableCollection<InfoBase> infoBases = new ObservableCollection<InfoBase>();
        private InfoBase _CurrentInfoBase = null;

        public MetadataTreeViewModel(IMetadataService dataService, IEventAggregator eventAggregator)
        {
            if (dataService == null) throw new ArgumentNullException("dataService");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            this.dataService = dataService;
            this.eventAggregator = eventAggregator;

            this.TreeViewDoubleClickCommand = new DelegateCommand<object>(this.OnTreeViewDoubleClick);

            RefreshInfoBases();
        }
        public ICommand TreeViewDoubleClickCommand { get; private set; }
        
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
        public InfoBase CurrentInfoBase
        {
            get { return _CurrentInfoBase; }
        }
        public void SetCurrentInfoBase(object model)
        {
            if (model is InfoBase)
            {
                _CurrentInfoBase = (InfoBase)model;
            }
            else if (model is Namespace)
            {
                _CurrentInfoBase = GetInfoBase((Namespace)model);
            }
            else if (model is Entity)
            {
                _CurrentInfoBase = GetInfoBase(((Entity)model).Namespace);
            }
            else if (model is Property)
            {
                _CurrentInfoBase = GetInfoBase(((Property)model).Entity.Namespace);
            }
            else if (model is Field)
            {
                _CurrentInfoBase = GetInfoBase(((Field)model).Property.Entity.Namespace);
            }
            else
            {
                _CurrentInfoBase = null;
            }
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
        private void OnTreeViewDoubleClick(object item)
        {
            SetCurrentInfoBase(item);
            this.eventAggregator.GetEvent<MetadataTreeViewItemSelected>().Publish(item);
        }
    }
}
