using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using System;
using System.Windows.Input;
using Zhichkin.Shell;

namespace Zhichkin.Hermes.UI
{
    public class QueryViewModel : BindableBase
    {
        private readonly IUnityContainer container;

        public QueryViewModel(IUnityContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            this.container = container;
            this.Name = "QueryViewModel";
        }

        public string Name { get; set; }
    }
}
