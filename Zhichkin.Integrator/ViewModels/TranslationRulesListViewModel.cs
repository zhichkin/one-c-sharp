using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Commands;
using Zhichkin.Metadata.Model;
using Zhichkin.ChangeTracking;
using Zhichkin.ORM;
using Zhichkin.Integrator.Model;
using Zhichkin.Integrator.Services;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace Zhichkin.Integrator.ViewModels
{
    public class TranslationRulesListViewModel : BindableBase
    {
        private readonly Subscription subscription;
        private readonly IRegionManager regionManager;
        private readonly IEventAggregator eventAggregator;
        private readonly IIntegratorService integrator = new IntegratorService();

        public TranslationRulesListViewModel(Subscription model, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            subscription = model;
            if (regionManager == null) throw new ArgumentNullException("regionManager");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;
            InitializeViewModel();
            this.DeleteTranslationRuleCommand = new DelegateCommand<TranslationRule>(this.OnDeleteTranslationRule);
            this.IsSyncKeyCheckCommand = new DelegateCommand<object>(this.OnIsSyncKeyChecked);
            this.IsSyncKeyUncheckCommand = new DelegateCommand<object>(this.OnIsSyncKeyUnchecked);
        }
        private void InitializeViewModel()
        {
            IList<TranslationRule> list = subscription.TranslationRules;
            translationRules = new ObservableCollection<TranslationRule>(list);

            foreach (Property property in subscription.Publisher.Entity.Properties)
            {
                TranslationRule rule = subscription.TranslationRules
                    .Where(i => i.SourceProperty == property).FirstOrDefault();
                if (rule == null)
                {
                    rule = subscription.CreateTranslationRule();
                    rule.SourceProperty = property;
                    translationRules.Add(rule);
                }
            }
        }
        private TranslationRule selectedItem = null;
        public TranslationRule SelectedItem
        {
            get { return selectedItem; }
            set { selectedItem = value; OnPropertyChanged("SelectedItem"); }
        }
        public ICommand DeleteTranslationRuleCommand { get; private set; }
        private ObservableCollection<TranslationRule> translationRules = null;
        public IList<TranslationRule> TranslationRules
        {
            get
            {
                if (translationRules == null)
                {
                    IList<TranslationRule> list = subscription.TranslationRules;
                    translationRules = new ObservableCollection<TranslationRule>(list);
                }
                return translationRules;
            }
        }
        private void OnDeleteTranslationRule(TranslationRule rule)
        {
            if (rule == null) return;
            if (rule.State == PersistentState.New)
            {
                rule.TargetProperty = null;
                rule.IsSyncKey = false;
            }
            else
            {
                rule.Kill();
                int index = translationRules.IndexOf(rule);
                Property property = rule.SourceProperty;
                rule = subscription.CreateTranslationRule();
                rule.SourceProperty = property;
                translationRules[index] = rule;
            }
        }
        public void SetTargetProperty(object value)
        {
            if (selectedItem == null) return;
            TranslationRule rule = selectedItem as TranslationRule;
            if (rule == null) return;
            
            Property property = value as Property;
            if (property == null) return;

            rule.TargetProperty = property;
            rule.IsSyncKey = rule.TargetProperty.Fields.Where(f => f.IsPrimaryKey).FirstOrDefault() != null;
            rule.Save();
        }
        public ICommand IsSyncKeyCheckCommand { get; private set; }
        private void OnIsSyncKeyChecked(object parameter)
        {
            TranslationRule rule = parameter as TranslationRule;
            if (rule == null) return;
            rule.IsSyncKey = true;
            rule.Save();
        }
        public ICommand IsSyncKeyUncheckCommand { get; private set; }
        private void OnIsSyncKeyUnchecked(object parameter)
        {
            TranslationRule rule = parameter as TranslationRule;
            if (rule == null) return;
            rule.IsSyncKey = false;
            rule.Save();
        }
    }
}
