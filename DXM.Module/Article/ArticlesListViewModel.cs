using System;
using System.Windows.Input;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System.Collections.ObjectModel;
using Zhichkin.Metadata.Model;
using Zhichkin.Shell;
using Microsoft.Practices.Unity;
using Zhichkin.DXM.Model;

namespace Zhichkin.DXM.Module
{
    public class ArticlesListViewModel : BindableBase
    {
        private readonly Publication _publication;
        private readonly IUnityContainer _container;
        private readonly IPublisherService _publisherService = new PublisherService();

        private ObservableCollection<Article> _Articles = new ObservableCollection<Article>();
        private ObservableCollection<Article> _Catalogs = new ObservableCollection<Article>();
        private ObservableCollection<Article> _Documents = new ObservableCollection<Article>();
        private ObservableCollection<Article> _InfoRegisters = new ObservableCollection<Article>();
        private ObservableCollection<Article> _AccumRegisters = new ObservableCollection<Article>();

        public ArticlesListViewModel(Publication publication, IUnityContainer container)
        {
            if (publication == null) throw new ArgumentNullException("publication");
            if (container == null) throw new ArgumentNullException("container");
            _publication = publication;
            _container = container;
            InitializeViewModel();
        }
        public void InitializeViewModel()
        {
            List<Article> list = _publisherService.GetArticles(_publication);
            foreach (Article article in list)
            {
                if (article.Entity.Owner != null && article.Entity.Owner.Namespace.Name == "Справочник")
                {
                    _Catalogs.Add(article);
                }
                else if (article.Entity.Owner != null && article.Entity.Owner.Namespace.Name == "Документ")
                {
                    _Documents.Add(article);
                }
                else if (article.Entity.Namespace.Name == "Справочник")
                {
                    _Catalogs.Add(article);
                }
                else if (article.Entity.Namespace.Name == "Документ")
                {
                    _Documents.Add(article);
                }
                else if (article.Entity.Namespace.Name == "РегистрСведений")
                {
                    _InfoRegisters.Add(article);
                }
                else if (article.Entity.Namespace.Name == "РегистрНакопления")
                {
                    _AccumRegisters.Add(article);
                }
                else
                {
                    _Articles.Add(article);
                }
            }
        }
        public ObservableCollection<Article> Articles { get { return _Articles; } }
        public ObservableCollection<Article> Catalogs { get { return _Catalogs; } }
        public ObservableCollection<Article> Documents { get { return _Documents; } }
        public ObservableCollection<Article> InfoRegisters { get { return _InfoRegisters; } }
        public ObservableCollection<Article> AccumRegisters { get { return _AccumRegisters; } }
        public void OnDrop(Entity entity)
        {
            if (_publication.Publisher != entity.InfoBase)
            {
                Z.Notify(new Notification()
                {
                    Title = Utilities.PopupDialogsTitle,
                    Content = "Информационные базы не совпадают!"
                });
                return;
            }
            Article article = _publisherService.GetArticle(_publication, entity);
            if (article != null)
            {
                Z.Notify(new Notification()
                    {
                        Title = Utilities.PopupDialogsTitle,
                        Content = "\"" + article.Name + "\" уже существует!"
                    });
                return;
            }
            article = _publisherService.Create(_publication, entity);
            if (article.Entity.Namespace.Name == "Справочник")
            {
                _Catalogs.Add(article);
            }
            else if (article.Entity.Namespace.Name == "Документ")
            {
                _Documents.Add(article);
            }
            else if (article.Entity.Namespace.Name == "РегистрСведений")
            {
                _InfoRegisters.Add(article);
            }
            else if (article.Entity.Namespace.Name == "РегистрНакопления")
            {
                _AccumRegisters.Add(article);
            }
            else
            {
                _Articles.Add(article);
            }
        }
    }
}
