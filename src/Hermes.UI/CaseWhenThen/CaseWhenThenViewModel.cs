using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Unity;
using System;

namespace Zhichkin.Hermes.UI
{
    public class CaseWhenThenViewModel : BindableBase
    {
        //private readonly IUnityContainer container;
        //public CaseWhenThenViewModel(IUnityContainer container)
        //{
        //    if (container == null) throw new ArgumentNullException("container");
        //    this.container = container;
        //}
        public CaseWhenThenViewModel() { }
        public string Name { get; set; }
    }
}
