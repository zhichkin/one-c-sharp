using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Commands;
using Zhichkin.Metadata.Model;
using Zhichkin.ORM;
using Zhichkin.Integrator.Model;
using Zhichkin.Integrator.Services;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Zhichkin.Shell;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace Zhichkin.Integrator.ViewModels
{
    public class TranslationRulesListViewModel : BindableBase
    {
        private const string CONST_ModuleDialogsTitle = "Z-Integrator";

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
            this.RefreshCommand = new DelegateCommand(this.OnRefresh);
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
        private string GetErrorText(Exception ex)
        {
            string errorText = string.Empty;
            Exception error = ex;
            while (error != null)
            {
                errorText += (errorText == string.Empty) ? error.Message : Environment.NewLine + error.Message;
                error = error.InnerException;
            }
            return errorText;
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
                try
                {
                    DeleteTranslationRule(rule);
                }
                catch (Exception ex)
                {
                    Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
                }
            }
        }
        private void DeleteTranslationRule(TranslationRule rule)
        {
            rule.Kill();
            int index = translationRules.IndexOf(rule);
            Property property = rule.SourceProperty;
            rule = subscription.CreateTranslationRule();
            rule.SourceProperty = property;
            translationRules[index] = rule;
        }
        public void SetTargetProperty(object value)
        {
            if (selectedItem == null) return;
            TranslationRule rule = selectedItem as TranslationRule;
            if (rule == null) return;
            
            Property property = value as Property;
            if (property == null) return;

            try
            {
                SaveTranslationRule(rule, property);
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            }
        }
        private void SaveTranslationRule(TranslationRule rule, Property property)
        {
            rule.TargetProperty = property;
            rule.IsSyncKey = rule.TargetProperty.Fields.Where(f => f.IsPrimaryKey).FirstOrDefault() != null;
            rule.Save();
        }
        public ICommand IsSyncKeyCheckCommand { get; private set; }
        private void OnIsSyncKeyChecked(object parameter)
        {
            TranslationRule rule = parameter as TranslationRule;
            if (rule == null) return;
            try
            {
                SwitchSyncKey(rule, true);
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            }
        }
        public ICommand IsSyncKeyUncheckCommand { get; private set; }
        private void OnIsSyncKeyUnchecked(object parameter)
        {
            TranslationRule rule = parameter as TranslationRule;
            if (rule == null) return;
            try
            {
                SwitchSyncKey(rule, false);
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = GetErrorText(ex) });
            }
        }
        private void SwitchSyncKey(TranslationRule rule, bool value)
        {
            if (rule.TargetProperty == null && value == false) return; // предотвращает бесконечный цикл изменения значения свойства =)
            if (rule.TargetProperty == null && value == true)
            {
                Z.Notify(new Notification { Title = CONST_ModuleDialogsTitle, Content = "Не заполнено свойство приёмника!" });
                rule.IsSyncKey = !value; // меняем значение свойства назад ... срабатывает OnPropertyChanged ...
                return;
            }
            rule.IsSyncKey = value;
            rule.Save();
        }
        public ICommand RefreshCommand { get; private set; }
        private void OnRefresh()
        {
            InitializeViewModel();
            OnPropertyChanged("TranslationRules");
        }
    }
}
