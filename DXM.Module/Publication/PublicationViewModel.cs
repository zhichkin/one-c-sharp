using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Zhichkin.DXM.Model;
using Zhichkin.Shell;

namespace Zhichkin.DXM.Module
{
    public class PublicationViewModel : BindableBase
    {
        private readonly Publication _publication;
        private readonly IUnityContainer _container;
        private readonly IRegionManager _regionManager;

        private string _Name = string.Empty;
        
        public PublicationViewModel(Publication model, IUnityContainer container, IRegionManager regionManager)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (container == null) throw new ArgumentNullException("container");
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            _publication = model;
            _container = container;
            _regionManager = regionManager;
            InitializeViewModel();
        }
        public void InitializeViewModel()
        {
            this.UpdateTextBoxSourceCommand = new DelegateCommand<object>(this.OnUpdateTextBoxSource);
            this.GoBackToInfoBaseViewCommand = new DelegateCommand(this.GoBackToInfoBaseView);
        }
        public ICommand UpdateTextBoxSourceCommand { get; private set; }
        public ICommand GoBackToInfoBaseViewCommand { get; private set; }
        private void OnUpdateTextBoxSource(object userControl)
        {
            TextBox textBox = userControl as TextBox;
            if (textBox == null) return;
            DependencyProperty property = TextBox.TextProperty;
            BindingExpression binding = BindingOperations.GetBindingExpression(textBox, property);
            if (binding == null) return;
            binding.UpdateSource();
        }
        public string Name
        {
            get { return _publication.Name; }
            set
            {
                try
                {
                    _Name = _publication.Name;
                    _publication.Name = value;
                    _publication.Save();
                    OnPropertyChanged("Name");
                }
                catch (Exception ex)
                {
                    _publication.Name = _Name;
                    _Name = string.Empty;
                    Z.Notify(new Notification { Title = Utilities.PopupDialogsTitle, Content = ExceptionsHandling.GetErrorText(ex) });
                }
            }
        }
        public string InfoBaseName { get { return _publication.Publisher.Name; } }
        private void GoBackToInfoBaseView()
        {
            Z.ClearRightRegion(_regionManager);
            IRegion rightRegion = _regionManager.Regions[RegionNames.RightRegion];
            if (rightRegion == null) return;
            object view = _container.Resolve(
                typeof(InfoBaseView),
                new ParameterOverride("model", _publication.Publisher)
                    .OnType(typeof(InfoBaseViewModel)));
            if (view == null) return;
            rightRegion.Add(view);
        }
    }
}
