using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
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
            
            this.SelectReferenceObjectDialog = new InteractionRequest<Confirmation>();
            this.BuildDependentNodesCommand = new DelegateCommand(this.OnBuildDependentNodes);
            this.SelectEntityReferenceCommand = new DelegateCommand(this.SelectEntityReference);
            this.RegisterEntitiesForExchangeCommand = new DelegateCommand(this.RegisterEntitiesForExchange);

            this.RegisterNodesReferencesForExchangeCommand = new DelegateCommand(this.OnRegisterNodesReferencesForExchange);
            this.RegisterNodesForeignReferencesForExchangeCommand = new DelegateCommand(this.OnRegisterNodesForeignReferencesForExchange);
            this.ExchangeDataCommand = new DelegateCommand(this.ExchangeData);

            this.RemoveCurrentNodeCommand = new DelegateCommand(this.OnRemoveCurrentNode);
            this.RegisterCurrentNodeReferencesCommand = new DelegateCommand(this.OnRegisterCurrentNodeReferences);
            this.RegisterCurrentNodeForeignReferencesCommand = new DelegateCommand(this.OnRegisterCurrentNodeForeignReferences);
            this.SendNodeRegistersToTargetCommand = new DelegateCommand(this.OnSendNodeRegistersToTarget);
            this.CreateCorrespondenceTablesCommand = new DelegateCommand(this.OnCreateCorrespondenceTables);
        }
        public ICommand ExchangeDataCommand { get; private set; }
        public ICommand SelectEntityReferenceCommand { get; private set; }
        public ICommand BuildDependentNodesCommand { get; private set; }
        public ICommand RegisterEntitiesForExchangeCommand { get; private set; }
        public ICommand RegisterNodesReferencesForExchangeCommand { get; private set; }
        public ICommand RegisterNodesForeignReferencesForExchangeCommand { get; private set; }

        public ICommand RemoveCurrentNodeCommand { get; private set; }
        public ICommand RegisterCurrentNodeReferencesCommand { get; private set; }
        public ICommand RegisterCurrentNodeForeignReferencesCommand { get; private set; }
        public ICommand SendNodeRegistersToTargetCommand { get; private set; }
        public ICommand CreateCorrespondenceTablesCommand { get; private set; }

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
        public MetadataTreeNode SelectedNode { get; set; }
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
            List<MetadataTreeNode> result = service.RegisterEntitiesForExchange(this.Nodes[0]);
            foreach (MetadataTreeNode node in result)
            {
                this.Nodes.Add(node);
            }
            //Z.Notify(new Notification { Title = "Hermes", Content = "Регистрация ссылок для обмена выполнена." });
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
        private void OnMetadataTreeViewSelectedItemChanged(object selectedItem)
        {
            TreeViewItem item = (TreeViewItem)selectedItem;
        }

        public void BuildDataNodesTree(Entity entity)
        {
            if (this.Department == null)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = "Не выбран филиал!" });
                return;
            }
            try
            {
                this.Nodes.Clear();
                DocumentsTreeService service = new DocumentsTreeService();
                service.Parameters.Add("Period", this.SelectedDate);
                service.Parameters.Add("Department", this.Department.Identity);
                service.BuildDocumentsTree(entity, new Progress<MetadataTreeNode>(OnDataNodesTreeBuilt));
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = Z.GetErrorText(ex) + Environment.NewLine + ex.StackTrace });
                return;
            }
        }
        public void OnDataNodesTreeBuilt(MetadataTreeNode node)
        {
            this.Nodes.Add(node);
        }

        private void OnBuildDependentNodes()
        {
            Entity entity = (Entity)this.SelectedNode.MetadataInfo;
            DocumentsTreeService service = new DocumentsTreeService();
            service.BuildDocumentsTree(entity, new Progress<MetadataTreeNode>(OnDependentNodesBuilt));
        }
        private void OnDependentNodesBuilt(MetadataTreeNode node)
        {
            this.SelectedNode.Children.Clear();
            foreach (MetadataTreeNode child in node.Children)
            {
                child.Parent = this.SelectedNode;
                this.SelectedNode.Children.Add(child);
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
            try
            {
                service.SendDataToTargetInfoBase();
                Z.Notify(new Notification { Title = "Hermes", Content = "Обмен данными выполнен." });
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = Z.GetErrorText(ex) + Environment.NewLine + ex.StackTrace });
            }
        }

        private void OnRegisterNodesReferencesForExchange()
        {
            if (this.SourceInfoBase == null)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = "Не выбран источник данных!" });
                return;
            }
            if (this.Department == null)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = "Не выбран филиал!" });
                return;
            }
            if (this.Nodes.Count == 0)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = "Не выбран узел метаданных!" });
                return;
            }
            DocumentsTreeService service = new DocumentsTreeService();
            MetadataTreeNode root = this.Nodes[0];
            this.Nodes.Clear();
            try
            {
                service.Parameters.Add("SourceInfoBase", this.SourceInfoBase);
                service.Parameters.Add("TargetInfoBase", this.TargetInfoBase);
                service.Parameters.Add("Period", this.SelectedDate);
                service.Parameters.Add("Department", this.Department.Identity);
                service.RegisterNodesReferencesForExchange(root, new Progress<MetadataTreeNode>(OnNodesReferencesRegistered));
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = Z.GetErrorText(ex) + Environment.NewLine + ex.StackTrace });
            }
        }
        private void OnNodesReferencesRegistered(MetadataTreeNode root)
        {
            this.Nodes.Add(root);
            Z.Notify(new Notification { Title = "Hermes", Content = "Ссылки зарегистрированы." });
        }

        private void OnRegisterNodesForeignReferencesForExchange()
        {
            if (this.SourceInfoBase == null)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = "Не выбран источник данных!" });
                return;
            }
            if (this.Department == null)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = "Не выбран филиал!" });
                return;
            }
            if (this.Nodes.Count == 0)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = "Не выбран узел метаданных!" });
                return;
            }
            DocumentsTreeService service = new DocumentsTreeService();
            MetadataTreeNode root = this.Nodes[0];
            try
            {
                service.Parameters.Add("SourceInfoBase", this.SourceInfoBase);
                service.Parameters.Add("TargetInfoBase", this.TargetInfoBase);
                service.Parameters.Add("Period", this.SelectedDate);
                service.Parameters.Add("Department", this.Department.Identity);
                service.RegisterNodesForeignReferencesForExchange(root, new Progress<MetadataTreeNode>(OnNodesForeignReferencesRegistered));
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = Z.GetErrorText(ex) + Environment.NewLine + ex.StackTrace });
            }
        }
        private void OnNodesForeignReferencesRegistered(MetadataTreeNode root)
        {
            Z.Notify(new Notification { Title = "Hermes", Content = "Внешние ссылки зарегистрированы." });
        }

        private void OnRemoveCurrentNode()
        {
            try
            {
                this.SelectedNode.Parent.Children.Remove(this.SelectedNode);
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = Z.GetErrorText(ex) + Environment.NewLine + ex.StackTrace });
            }
        }
        private void OnRegisterCurrentNodeReferences()
        {
            DocumentsTreeService service = new DocumentsTreeService();
            try
            {
                service.RegisterCurrentNodeReferencesForExchange(this.SelectedNode, new Progress<MetadataTreeNode>(ReportProgress));
                Z.Notify(new Notification { Title = "Hermes", Content = "Ссылки зарегистрированы." });
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = Z.GetErrorText(ex) + Environment.NewLine + ex.StackTrace });
            }
        }
        private void OnRegisterCurrentNodeForeignReferences()
        {
            DocumentsTreeService service = new DocumentsTreeService();
            service.Parameters.Add("Period", this.SelectedDate);
            service.Parameters.Add("Department", this.Department.Identity);
            try
            {
                service.RegisterCurrentNodeForeignReferencesForExchange(this.SelectedNode, new Progress<MetadataTreeNode>(ReportProgress));
                Z.Notify(new Notification { Title = "Hermes", Content = "Внешние ссылки зарегистрированы." });
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = Z.GetErrorText(ex) + Environment.NewLine + ex.StackTrace });
            }
        }
        private void OnSendNodeRegistersToTarget()
        {
            DocumentsTreeService service = new DocumentsTreeService();
            service.Parameters.Add("SourceInfoBase", this.SourceInfoBase);
            service.Parameters.Add("TargetInfoBase", this.TargetInfoBase);
            try
            {
                service.SendNodeRegistersToTarget(this.SelectedNode, new Progress<MetadataTreeNode>(ReportProgress));
                Z.Notify(new Notification { Title = "Hermes", Content = "Перенос данных узла выполнен" });
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = Z.GetErrorText(ex) + Environment.NewLine + ex.StackTrace });
            }
        }
        private void OnCreateCorrespondenceTables()
        {
            DocumentsTreeService service = new DocumentsTreeService();
            service.Parameters.Add("SourceInfoBase", this.SourceInfoBase);
            service.Parameters.Add("TargetInfoBase", this.TargetInfoBase);
            try
            {
                service.CreateCorrespondenceTablesAndFunctions();
                Z.Notify(new Notification { Title = "Hermes", Content = "Таблицы соответствий созданы" });
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = Z.GetErrorText(ex) + Environment.NewLine + ex.StackTrace });
            }
        }
        private void ReportProgress(MetadataTreeNode node)
        {

        }
    }
}
