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
            this.RegisterEntitiesForExchangeCommand = new DelegateCommand(this.RegisterEntitiesForExchange);
        }
        public ICommand RegisterEntitiesForExchangeCommand { get; private set; }
        public ObservableCollection<MetadataTreeNode> Nodes { get; set; }
        public DateTime SelectedDate { get; set; }
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
            if (this.Nodes.Count == 0)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = "Не выбран узел данных!" });
                return;
            }
            DocumentsTreeService service = new DocumentsTreeService();
            service.Parameters.Add("Period", this.SelectedDate);
            List<MetadataTreeNode> result = service.RegisterEntitiesForExchange(this.Nodes[0]);
            foreach (MetadataTreeNode node in result)
            {
                this.Nodes.Add(node);
            }
            Z.Notify(new Notification { Title = "Hermes", Content = "Регистрация ссылок для обмена выполнена." });
        }
    }
}
