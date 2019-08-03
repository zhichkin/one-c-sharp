using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Zhichkin.Hermes.Model;
using Zhichkin.Hermes.Services;
using Zhichkin.Hermes.UI;
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
            this.NamespaceViewPopup = new InteractionRequest<Confirmation>();
            this.EntityPopup = new InteractionRequest<Confirmation>();
            this.PropertyPopup = new InteractionRequest<Confirmation>();
            this.EntityViewPopup = new InteractionRequest<Confirmation>();
            this.RequestViewPopup = new InteractionRequest<Confirmation>();

            this.SetupPropertyContextMenu();

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

        public InteractionRequest<Confirmation> NamespaceViewPopup { get; private set; }
        public void OpenNamespaceView(object model)
        {
            Confirmation confirmation = new Confirmation()
            {
                Title = "Z-Metadata",
                Content = (Namespace)model
            };
            this.NamespaceViewPopup.Raise(confirmation);
        }
        public void CreateNewNamespace(object owner)
        {
            Namespace new_namespace = new Namespace();
            if (owner is InfoBase)
            {
                new_namespace.Owner = (InfoBase)owner;
            }
            else if (owner is Namespace)
            {
                new_namespace.Owner = (Namespace)owner;
            }
            else
            {
                throw new ArgumentOutOfRangeException("owner");
            }

            Confirmation confirmation = new Confirmation()
            {
                Title = "Z-Metadata",
                Content = new_namespace
            };
            this.NamespaceViewPopup.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    Namespace ns = response.Content as Namespace;
                    if (ns != null)
                    {
                        if (ns.Owner is InfoBase)
                        {
                            ns.InfoBase.ObservableNamespaces.Add(ns);
                            //ns.InfoBase.OnPropertyChanged("Namespaces");
                        }
                        else if (ns.Owner is Namespace)
                        {
                            ((Namespace)ns.Owner).ObservableNamespaces.Add(ns);
                            //((Namespace)ns.Owner).OnPropertyChanged("Namespaces");
                        }
                    }
                }
            });
        }
        public void KillNamespace(object model)
        {
            Namespace ns = model as Namespace;
            if (ns == null) throw new ArgumentNullException("model");

            bool cancel = true;
            Z.Confirm(new Confirmation
                {
                    Title = "Z-Metadata",
                    Content = $"Пространство имён \"{ns.Name}\" и все его\nподчинённые объекты будут удалены.\n\nПродолжить ?"
                },
                c => { cancel = !c.Confirmed; });

            if (cancel) return;

            try
            {
                TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, options))
                {
                    dataService.Kill(ns);
                    scope.Complete();
                }
                if (ns.Owner is InfoBase)
                {
                    InfoBase ib = this.InfoBases.Where(i => i == ns.Owner).FirstOrDefault();
                    if (ib != null)
                    {
                        ib.ObservableNamespaces.Remove(ns);
                    }
                }
                else if (ns.Owner is Namespace)
                {
                    InfoBase ib = this.InfoBases.Where(i => i == ns.InfoBase).FirstOrDefault();
                    if (ib != null)
                    {
                        Namespace owner = FindObservableNamespace(ib, (Namespace)ns.Owner);
                        if (owner != null)
                        {
                            owner.ObservableNamespaces.Remove(ns);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = Z.GetErrorText(ex) });
            }
        }
        private Namespace FindObservableNamespace(InfoBase ib, Namespace ns)
        {
            Namespace found = null;
            foreach (Namespace n in ib.ObservableNamespaces)
            {
                if (n == ns)
                {
                    found = n;
                    break;
                }
                found = FindObservableNamespace(n, ns);
                if (found != null)
                {
                    return found;
                }
            }
            return found;
        }
        private Namespace FindObservableNamespace(Namespace parent, Namespace ns)
        {
            Namespace found = null;
            foreach (Namespace n in parent.ObservableNamespaces)
            {
                if (n == ns)
                {
                    found = n;
                    break;
                }
                found = FindObservableNamespace(n, ns);
                if (found != null)
                {
                    return found;
                }
            }
            return found;
        }

        public InteractionRequest<Confirmation> PropertyPopup { get; private set; }
        public void OpenPropertyForm(object model)
        {
            Property property = model as Property;
            if (property == null) return;

            Confirmation confirmation = new Confirmation()
            {
                Title = "Z-Metadata",
                Content = property
            };
            this.PropertyPopup.Raise(confirmation);

            property.OnPropertyChanged("Name");
        }
        public void CreateNewProperty(object owner)
        {
            Property new_property = new Property();
            if (owner is Entity)
            {
                new_property.Entity = (Entity)owner;
                new_property.Purpose = PropertyPurpose.Property;
                new_property.Ordinal = new_property.Entity.Properties.Count;
                new_property.Name = $"NewProperty{new_property.Ordinal}";
            }
            else
            {
                throw new ArgumentOutOfRangeException("owner");
            }

            Confirmation confirmation = new Confirmation()
            {
                Title = "Z-Metadata",
                Content = new_property
            };
            this.PropertyPopup.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    Property property = response.Content as Property;
                    if (property != null)
                    {
                        property.Entity.ObservableProperties.Add(property);
                    }
                }
            });
        }
        public void KillProperty(object model)
        {
            Property property = model as Property;
            if (property == null) throw new ArgumentNullException("model");

            bool cancel = true;
            Z.Confirm(new Confirmation
            {
                Title = "Z-Metadata",
                Content = $"Свойство \"{property.Name}\" и все его\nподчинённые объекты будут удалены.\n\nПродолжить ?"
            },
                c => { cancel = !c.Confirmed; });

            if (cancel) return;

            try
            {
                TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, options))
                {
                    dataService.Kill(property);
                    scope.Complete();
                }
                InfoBase ib = this.InfoBases.Where(i => i == property.Entity.InfoBase).FirstOrDefault();
                if (ib != null)
                {
                    Namespace ns = FindObservableNamespace(ib, property.Entity.Namespace);
                    if (ns != null)
                    {
                        Entity owner = ns.ObservableEntities.Where(e => e == property.Entity).FirstOrDefault();
                        if (owner != null)
                        {
                            owner.ObservableProperties.Remove(property);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = Z.GetErrorText(ex) });
            }
        }

        public InteractionRequest<Confirmation> EntityPopup { get; private set; }
        public void OpenEntityForm(object model)
        {
            Entity entity = model as Entity;
            if (entity == null) return;

            Confirmation confirmation = new Confirmation()
            {
                Title = "Z-Metadata",
                Content = entity
            };
            this.EntityPopup.Raise(confirmation);

            InfoBase ib = this.InfoBases.Where(i => i == entity.InfoBase).FirstOrDefault();
            if (ib != null)
            {
                Namespace ns = FindObservableNamespace(ib, entity.Namespace);
                if (ns != null)
                {
                    Entity owner = ns.ObservableEntities.Where(e => e == entity).FirstOrDefault();
                    if (owner != null)
                    {
                        foreach (Property property in owner.ObservableProperties)
                        {
                            property.Load();
                            property.OnPropertyChanged("Name");
                        }
                    }
                }
            }
        }
        public void CreateNewEntity(object model)
        {
            Entity entity = new Entity();
            if (model is Namespace)
            {
                entity.Namespace = (Namespace)model;
                entity.Name = $"NewEntity{entity.Namespace.Entities.Count.ToString()}";
                //TODO: entity.Parent = Entity.Object; !?
            }
            else
            {
                throw new ArgumentOutOfRangeException("owner");
            }

            Confirmation confirmation = new Confirmation()
            {
                Title = "Z-Metadata",
                Content = entity
            };
            this.EntityPopup.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    Entity content = response.Content as Entity;
                    if (content != null)
                    {
                        content.Namespace.ObservableEntities.Add(content);
                        //content.Namespace.Entities.Add(content);
                        //content.Namespace.OnPropertyChanged("Entities");
                    }
                }
            });
        }
        public void CreateNewNestedEntity(object model)
        {
            Entity entity = new Entity();
            if (model is Entity)
            {
                entity.Owner = (Entity)model;
                entity.Namespace = entity.Owner.Namespace;
                entity.Name = $"NewNestedEntity{entity.Owner.NestedEntities.Count.ToString()}";
                //TODO: entity.Parent = Entity.Object; !?
            }
            else
            {
                throw new ArgumentOutOfRangeException("owner");
            }

            Confirmation confirmation = new Confirmation()
            {
                Title = "Z-Metadata",
                Content = entity
            };
            this.EntityPopup.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    Entity content = response.Content as Entity;
                    if (content != null)
                    {
                        content.Owner.NestedEntities.Add(content);
                        content.Owner.OnPropertyChanged("NestedEntities");
                    }
                }
            });
        }
        public void KillEntity(object model)
        {
            Entity entity = model as Entity;
            if (entity == null) throw new ArgumentNullException("model");

            bool cancel = true;
            Z.Confirm(new Confirmation
            {
                Title = "Z-Metadata",
                Content = $"Сущность \"{entity.Name}\" и все её\nподчинённые объекты будут удалены.\n\nПродолжить ?"
            },
                c => { cancel = !c.Confirmed; });

            if (cancel) return;

            try
            {
                TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, options))
                {
                    dataService.Kill(entity);
                    scope.Complete();
                }
                if (entity.Owner != null)
                {
                    entity.Owner.NestedEntities.Remove(entity);
                    entity.Owner.OnPropertyChanged("NestedEntities");
                }
                else
                {
                    InfoBase ib = this.InfoBases.Where(i => i == entity.InfoBase).FirstOrDefault();
                    if (ib != null)
                    {
                        Namespace owner = FindObservableNamespace(ib, entity.Namespace);
                        if (owner != null)
                        {
                            owner.ObservableEntities.Remove(entity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = Z.GetErrorText(ex) });
            }
        }

        public InteractionRequest<Confirmation> EntityViewPopup { get; private set; }
        public void OpenEntityView(object model)
        {
            Confirmation confirmation = new Confirmation()
            {
                Title = "Z-Metadata",
                Content = (Entity)model
            };
            this.EntityViewPopup.Raise(confirmation);
        }

        public ObservableCollection<MetadataCommandViewModel> PropertyContextMenuItems { get; private set; }
        private void SetupPropertyContextMenu()
        {
            List<MetadataCommandViewModel> commandsList = new List<MetadataCommandViewModel>();

            commandsList.Add(new MetadataCommandViewModel()
            {
                Name = "Открыть",
                Icon = "Icon_Settings", // pack://application:,,,/Zhichkin.Metadata.Module;component/views/metadatatreeview.xaml	System.Windows.Media.Imaging.BitmapImage
                Command = new DelegateCommand<object>(this.OpenPropertyForm)
            });
            commandsList.Add(new MetadataCommandViewModel()
            {
                Name = "Удалить",
                Icon = "Icon_Kill_Object",
                Command = new DelegateCommand<object>(this.KillProperty)
            });
            commandsList.Add(new MetadataCommandViewModel() { Name = "-" }); // Separator
            commandsList.Add(new MetadataCommandViewModel()
            {
                Name = "Редактировать запрос",
                Icon = "Icon_Edit_Object",
                Command = new DelegateCommand<object>(this.OpenQueryDesigner)
            });

            this.PropertyContextMenuItems = new ObservableCollection<MetadataCommandViewModel>(commandsList);
        }

        private void OpenQueryDesigner(object model)
        {
            Request request = model as Request;
            if (request == null) return;

            Z.ClearRightRegion(this.regionManager);
            IRegion rightRegion = this.regionManager.Regions[RegionNames.RightRegion];
            if (rightRegion == null) return;

            QueryExpression query;
            QueryExpressionViewModel queryVM;
            if (string.IsNullOrEmpty(request.ParseTree))
            {
                query = new QueryExpression(null, request);
                query.Expressions = new List<HermesModel>();
                queryVM = new QueryExpressionViewModel(null, query);

                SelectStatement statement = new SelectStatement(query, null);
                query.Expressions.Add(statement);
                SelectStatementViewModel select = new SelectStatementViewModel(queryVM, statement);
                queryVM.QueryExpressions.Add(select);
            }
            else
            {
                ISerializationService serializer = container.Resolve<ISerializationService>();
                query = serializer.FromJson(request.ParseTree);
                query.Request = request;
                queryVM = new QueryExpressionViewModel(null, query);
            }

            QueryExpressionView queryView = new QueryExpressionView(queryVM);
            rightRegion.Add(queryView);
        }

        public InteractionRequest<Confirmation> RequestViewPopup { get; private set; }
        public void CreateNewRequest(object owner)
        {
            Request request = new Request();

            if (owner is Namespace)
            {
                request.Namespace = (Namespace)owner;
                request.Name = $"NewRequest{request.Namespace.Requests.Count.ToString()}";
            }
            else if (owner is Entity)
            {
                request.Owner = (Entity)owner;
                request.Name = $"NewRequest{request.Owner.Requests.Count.ToString()}";
            }
            else
            {
                throw new ArgumentOutOfRangeException("owner");
            }

            Confirmation confirmation = new Confirmation()
            {
                Title = "Z-Metadata",
                Content = request
            };
            this.RequestViewPopup.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    Request content = response.Content as Request;
                    if (content != null)
                    {
                        if (owner is Namespace)
                        {
                            content.Namespace.ObservableRequests.Add(content);
                        }
                        else if (owner is Entity)
                        {
                            content.Owner.ObservableRequests.Add(content);
                        }
                    }
                }
            });
        }
        public void OpenRequestView(object model)
        {
            Request request = model as Request;
            if (request == null) return;

            Confirmation confirmation = new Confirmation()
            {
                Title = "Z-Metadata",
                Content = request
            };
            this.RequestViewPopup.Raise(confirmation);
        }
        public void KillRequest(object model)
        {
            Request request = model as Request;
            if (request == null) throw new ArgumentNullException("model");

            bool cancel = true;
            Z.Confirm(new Confirmation
            {
                Title = "Z-Metadata",
                Content = $"Запрос данных \"{request.Name}\" и все его\nподчинённые объекты будут удалены.\n\nПродолжить ?"
            },
                c => { cancel = !c.Confirmed; });

            if (cancel) return;

            try
            {
                TransactionOptions options = new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted };
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, options))
                {
                    dataService.Kill(request);
                    scope.Complete();
                }
                if (request.Namespace != null)
                {
                    InfoBase ib = this.InfoBases.Where(i => i == request.Namespace.InfoBase).FirstOrDefault();
                    if (ib != null)
                    {
                        Namespace owner = FindObservableNamespace(ib, request.Namespace);
                        if (owner != null)
                        {
                            owner.ObservableRequests.Remove(request);
                        }
                    }
                }
                if (request.Owner != null)
                {
                    InfoBase ib = this.InfoBases.Where(i => i == request.Owner.InfoBase).FirstOrDefault();
                    if (ib != null)
                    {
                        Namespace ns = FindObservableNamespace(ib, request.Owner.Namespace);
                        if (ns != null)
                        {
                            Entity owner = ns.ObservableEntities.Where(e => e == request.Owner).FirstOrDefault();
                            if (owner != null)
                            {
                                owner.ObservableRequests.Remove(request);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Z-Metadata", Content = Z.GetErrorText(ex) });
            }
        }
        public void EditRequest(object model)
        {
            this.OpenQueryDesigner(model);
        }
    }
}
