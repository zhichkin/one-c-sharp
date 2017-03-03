using System;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Commands;
using Zhichkin.Metadata.Model;
using Zhichkin.Shell;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;

namespace Zhichkin.DXM.Module
{
    public class InfoBaseViewModel : BindableBase
    {
        private const string CONST_ModuleDialogsTitle = "Z-DXM";

        private readonly InfoBase _infoBase;
        private readonly IRegionManager _regionManager;
        
        public InfoBaseViewModel(InfoBase model, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            _infoBase = model;
            _regionManager = regionManager;
            InitializeViewModel();
        }
        public void InitializeViewModel()
        {
            this.UpdateTextBoxSourceCommand = new DelegateCommand<object>(this.OnUpdateTextBoxSource);
        }
        public ICommand UpdateTextBoxSourceCommand { get; private set; }
        private void OnUpdateTextBoxSource(object userControl)
        {
            TextBox textBox = userControl as TextBox;
            if (textBox == null) return;
            DependencyProperty property = TextBox.TextProperty;
            BindingExpression binding = BindingOperations.GetBindingExpression(textBox, property);
            if (binding == null) return;
            binding.UpdateSource();
        }
        
        private string _Name = string.Empty;
        private string _Server = string.Empty;
        private string _Database = string.Empty;
        public string Name
        {
            get { return _infoBase.Name; }
            set
            {
                try
                {
                    _Name = _infoBase.Name;
                    _infoBase.Name = value;
                    _infoBase.Save();
                    OnPropertyChanged("Name");
                }
                catch (Exception ex)
                {
                    _infoBase.Name = _Name;
                    _Name = string.Empty;
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = ExceptionsHandling.GetErrorText(ex) });
                }
            }
        }
        public string Server
        {
            get { return _infoBase.Server; }
            set
            {
                try
                {
                    _Server = _infoBase.Server;
                    _infoBase.Server = value;
                    _infoBase.Save();
                    OnPropertyChanged("Server");
                }
                catch (Exception ex)
                {
                    _infoBase.Server = _Server;
                    _Server = string.Empty;
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = ExceptionsHandling.GetErrorText(ex) });
                }
            }
        }
        public string Database
        {
            get { return _infoBase.Database; }
            set
            {
                try
                {
                    _Database = _infoBase.Database;
                    _infoBase.Database = value;
                    _infoBase.Save();
                    OnPropertyChanged("Database");
                }
                catch (Exception ex)
                {
                    _infoBase.Database = _Database;
                    _Database = string.Empty;
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = ExceptionsHandling.GetErrorText(ex) });
                }
            }
        }
    }
}
