using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Unity;
using System;
using Zhichkin.DXM.Model;

namespace Zhichkin.DXM.Module
{
    public class PublicationMainViewModel : BindableBase
    {
        private readonly Publication _publication;
        private readonly IUnityContainer _container;
        private readonly IPublisherService _publisherService = new PublisherService();

        private ArticlesListView _ArticlesListView;
        private ArticleFiltersListView _ArticleFiltersListView;

        public PublicationMainViewModel(Publication publication, IUnityContainer container)
        {
            if (publication == null) throw new ArgumentNullException("publication");
            if (container == null) throw new ArgumentNullException("container");
            _publication = publication;
            _container = container;
            InitializeViewModel();
        }
        public void InitializeViewModel()
        {
            _ArticlesListView = (ArticlesListView)_container.Resolve(
                typeof(ArticlesListView),
                new ParameterOverride("publication", _publication)
                    .OnType(typeof(ArticlesListViewModel)));

            _ArticleFiltersListView = (ArticleFiltersListView)_container.Resolve(
                typeof(ArticleFiltersListView),
                new ParameterOverride("article", new InjectionParameter<Article>(null))
                    .OnType(typeof(ArticleFiltersListViewModel)));
        }
        public ArticlesListView ArticlesListView { get { return _ArticlesListView; } }
        public ArticleFiltersListView ArticleFiltersListView { get { return _ArticleFiltersListView; } }
    }
}
