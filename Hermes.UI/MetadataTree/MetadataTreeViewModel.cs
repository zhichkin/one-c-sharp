using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Zhichkin.Hermes.Infrastructure;
using Zhichkin.Hermes.Services;
using Zhichkin.Metadata.Model;
using Zhichkin.Metadata.Services;
using Zhichkin.Shell;

namespace Zhichkin.Hermes.UI
{
    public class MetadataTreeViewModel : BindableBase
    {
        public MetadataTreeViewModel()
        {
            this.SelectedDate = DateTime.Now;
            this.Nodes = new ObservableCollection<MetadataTreeNode>();
            this.StateList = new ObservableCollection<string>();
            this.ExchangeDataCommand = new DelegateCommand(this.ExchangeData);
            this.SelectReferenceObjectDialog = new InteractionRequest<Confirmation>();
            this.SelectEntityReferenceCommand = new DelegateCommand(this.SelectEntityReference);
            this.RegisterEntitiesForExchangeCommand = new DelegateCommand(this.RegisterEntitiesForExchange);
        }
        public ICommand ExchangeDataCommand { get; private set; }
        public ICommand SelectEntityReferenceCommand { get; private set; }
        public ICommand RegisterEntitiesForExchangeCommand { get; private set; }
        public InteractionRequest<Confirmation> SelectReferenceObjectDialog { get; private set; }
        public ObservableCollection<MetadataTreeNode> Nodes { get; set; }
        public List<InfoBase> InfoBases
        {
            get
            {
                MetadataService service = new MetadataService();
                return service.GetInfoBases();
            }
        }
        public InfoBase SourceInfoBase { get; set; }
        public InfoBase TargetInfoBase { get; set; }
        public DateTime SelectedDate { get; set; }
        private string _DepartmentName = "Выберите филиал ...";
        public ReferenceProxy Department { get; set; }
        public string DepartmentName
        {
            get { return _DepartmentName; }
            set
            {
                _DepartmentName = value;
                OnPropertyChanged("DepartmentName");
            }
        }
        public void SetStateText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            this.StateList.Add(text);
        }
        public ObservableCollection<string> StateList { get; }
        private void RegisterEntitiesForExchange()
        {
            if (this.SourceInfoBase == null)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = "Не выбрана информационная база данных!" });
                return;
            }
            if (this.Department == null)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = "Не выбран филиал!" });
                return;
            }
            if (this.Nodes.Count == 0)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = "Не выбран узел данных!" });
                return;
            }
            DocumentsTreeService service = new DocumentsTreeService();
            service.Parameters.Add("Period", this.SelectedDate);
            service.Parameters.Add("Department", this.Department.Identity);
            List<MetadataTreeNode> result = service.RegisterEntitiesForExchange(this.Nodes[0]);
            foreach (MetadataTreeNode node in result)
            {
                this.Nodes.Add(node);
            }
            Z.Notify(new Notification { Title = "Hermes", Content = "Регистрация ссылок для обмена выполнена." });
        }
        private void SelectEntityReference()
        {
            if (this.SourceInfoBase == null)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = "Не выбрана информационная база данных!" });
                return;
            }
            MetadataService service = new MetadataService();
            Entity entity = service.GetEntityInfo(this.SourceInfoBase, "Справочник", "яФилиалы");
            Confirmation confirmation = new Confirmation() { Title = string.Empty, Content = entity };
            this.SelectReferenceObjectDialog.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    this.Department = response.Content as ReferenceProxy;
                    if (this.Department != null)
                    {
                        this.DepartmentName = this.Department.ToString();
                    }
                }
            });
        }
        public void OnMetadataTreeViewItemSelected(object item)
        {
            try
            {
                InfoBase infoBase = item as InfoBase;
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = ex.Message });
            }
        }

        public void ExchangeData()
        {
            if (this.SourceInfoBase == null)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = "Не выбран источник данных!" });
                return;
            }
            if (this.TargetInfoBase == null)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = "Не выбран приёмник данных!" });
                return;
            }
            if (this.SourceInfoBase == this.TargetInfoBase)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = "Источник и приёмник не могут быть равны!" });
                return;
            }
            DocumentsTreeService service = new DocumentsTreeService();
            service.Parameters.Add("SourceInfoBase", this.SourceInfoBase);
            service.Parameters.Add("TargetInfoBase", this.TargetInfoBase);
            service.SendDataToTargetInfoBase();
            Z.Notify(new Notification { Title = "Hermes", Content = "Обмен данными выполнен." });
        }
    }
}
