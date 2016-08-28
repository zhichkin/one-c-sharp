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
        }
        public ObservableCollection<InfoBase> InfoBases
        {
            get
            {
                return infoBases;
            }
        }
        public void SelectedItemChanged(object item)
        {
            this.eventAggregator.GetEvent<MetadataTreeViewItemSelected>().Publish(item);
        }
    }
}
