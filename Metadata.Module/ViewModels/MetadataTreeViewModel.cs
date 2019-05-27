using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.Module;
using Zhichkin.Metadata.Services;
using Zhichkin.Metadata.UI;
using Zhichkin.Metadata.Views;
using Zhichkin.Shell;

namespace Zhichkin.Metadata.ViewModels
{
    public class MetadataTreeViewModel : BindableBase
    {
        private readonly IUnityContainer container;
        private readonly IRegionManager regionManager;
        private readonly IMetadataService dataService;
        private readonly IEventAggregator eventAggregator;

        private readonly Dictionary<Type, Type> viewsLookup = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, Type> modelsLookup = new Dictionary<Type, Type>();

        private ObservableCollection<InfoBase> infoBases = new ObservableCollection<InfoBase>();
        private InfoBase _CurrentInfoBase = null;

        public MetadataTreeViewModel(IMetadataService dataService, IEventAggregator eventAggregator,
            IUnityContainer container, IRegionManager regionManager)
        {
            if (container == null) throw new ArgumentNullException("container");
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            if (dataService == null) throw new ArgumentNullException("dataService");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            this.container = container;
            this.regionManager = regionManager;
            this.dataService = dataService;
            this.eventAggregator = eventAggregator;
            this.TreeViewDoubleClickCommand = new DelegateCommand<object>(this.OnTreeViewDoubleClick);

            viewsLookup.Add(typeof(Entity), typeof(EntityView));
            modelsLookup.Add(typeof(Entity), typeof(EntityViewModel));
            viewsLookup.Add(typeof(Property), typeof(PropertyView));
            modelsLookup.Add(typeof(Property), typeof(PropertyViewModel));

            this.InfoBaseViewPopup = new InteractionRequest<Confirmation>();

            RefreshInfoBases();
        }
        public ICommand ShowMetadataProperties { get; private set; }
        public ICommand TreeViewDoubleClickCommand { get; private set; }

        private void RefreshInfoBases()
        {
            this.infoBases.Clear();
            try
            {
                InfoBase system = dataService.GetSystemInfoBase();
                this.InfoBases.Add(system);

                foreach (InfoBase infoBase in dataService.GetInfoBases())
                {
                    this.infoBases.Add(infoBase);
                }
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = ExceptionsHandling.GetErrorText(ex) });
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
                _CurrentInfoBase = GetInfoBase(((Field)model).Table.Entity.Namespace);
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

        private object GetView(object model)
        {
            Type itemType = model.GetType();
            Type viewType = null;
            viewsLookup.TryGetValue(itemType, out viewType);
            if (viewType == null) return null;
            Type modelType = modelsLookup[itemType];
            return this.container.Resolve(viewType, new ParameterOverride("model", model).OnType(modelType));
        }
        public void ShowProperties(object model)
        {
            if (model == null) return;
            try
            {
                Z.ClearRightRegion(regionManager);
                if (model == null) return;
                IRegion rightRegion = this.regionManager.Regions[RegionNames.RightRegion];
                if (rightRegion == null) return;
                object view = GetView(model);
                if (view == null) return;
                rightRegion.Add(view);
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = ExceptionsHandling.GetErrorText(ex) });
            }
        }

        public InteractionRequest<Confirmation> InfoBaseViewPopup { get; private set; }
        public void OpenInfoBaseView(object model)
        {
            Confirmation confirmation = new Confirmation()
            {
                Title = "Z-Metadata",
                Content = (InfoBase)model
            };
            this.InfoBaseViewPopup.Raise(confirmation);
        }
        public void KillInfoBase(object model)
        {
            _CurrentInfoBase = (InfoBase)model;
            MainMenuViewModel viewModel = this.GetMetadataModuleMainMenu();
            if (viewModel == null) return;
            viewModel.KillMetadataCommand.Execute(model);
        }
        private MainMenuViewModel GetMetadataModuleMainMenu()
        {
            IRegion topRegion = this.regionManager.Regions[RegionNames.TopRegion];
            if (topRegion == null) return null;
            MetadataMainMenu view = topRegion.Views.Where(v => v is MetadataMainMenu).FirstOrDefault() as MetadataMainMenu;
            if (view == null) return null;
            return view.DataContext as MainMenuViewModel;
        }
    }
}
