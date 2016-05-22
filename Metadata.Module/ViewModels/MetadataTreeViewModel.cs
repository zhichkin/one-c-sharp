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
        private IMetadataService service;

        private ObservableCollection<InfoBase> infoBases = null;
        
        public MetadataTreeViewModel(IMetadataService service)
        {
            this.service = service;
        }
        public ObservableCollection<InfoBase> InfoBases
        {
            get
            {
                if (infoBases == null)
                {
                    infoBases = new ObservableCollection<InfoBase>();
                    infoBases.Add(service.GetMetadata(@"Srvr=""WINGYACE-HP"";Ref=""TRADE"";"));
                }
                return infoBases;
            }
        }
    }
}
