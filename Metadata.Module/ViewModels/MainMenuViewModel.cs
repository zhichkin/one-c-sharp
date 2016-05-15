using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Zhichkin.Metadata.Controllers;

using Zhichkin.Metadata.Commands;

namespace Zhichkin.Metadata.ViewModels
{
    public class MainMenuViewModel : BindableBase
    {
        private readonly IEventAggregator eventAggregator;

        public MainMenuViewModel(IEventAggregator eventAggregator)
        {
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");

            this.eventAggregator = eventAggregator;

            OpenMetadataCommand = new OpenMetadataCommand<object>(this.OnOpenMetadata, this.CanExecuteCommand);
            SaveMetadataCommand = new SaveMetadataCommand<object>(this.OnSaveMetadata, this.CanExecuteCommand);
            KillMetadataCommand = new KillMetadataCommand<object>(this.OnKillMetadata, this.CanExecuteCommand);
            UpdateMetadataCommand = new UpdateMetadataCommand<object>(this.OnUpdateMetadata, this.CanExecuteCommand);
            ShowSettingsCommand = new ShowSettingsCommand<object>(this.OnShowSettings, this.CanExecuteCommand);
        }

        public ICommand OpenMetadataCommand { get; private set; }
        public ICommand SaveMetadataCommand { get; private set; }
        public ICommand KillMetadataCommand { get; private set; }
        public ICommand UpdateMetadataCommand { get; private set; }
        public ICommand ShowSettingsCommand { get; private set; }

        private bool CanExecuteCommand(object args) { return true; }

        private void OnOpenMetadata(object args)
        {
            this.eventAggregator.GetEvent<MetadataTreeViewItemSelected>().Publish(args);
        }
        private void OnSaveMetadata(object args)
        {
            this.eventAggregator.GetEvent<MetadataTreeViewItemSelected>().Publish(args);
        }
        private void OnKillMetadata(object args)
        {
            this.eventAggregator.GetEvent<MetadataTreeViewItemSelected>().Publish(args);
        }
        private void OnUpdateMetadata(object args)
        {
            this.eventAggregator.GetEvent<MetadataTreeViewItemSelected>().Publish(args);
        }
        private void OnShowSettings(object args)
        {
            this.eventAggregator.GetEvent<MetadataTreeViewItemSelected>().Publish(args);
            MessageBox.Show("Settings");
        }
    }
}
