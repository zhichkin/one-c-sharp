using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.Mvvm;
using Zhichkin.Metadata.Commands;

namespace Zhichkin.Metadata.ViewModel
{
    public class MetadataMainMenu : BindableBase
    {
        public MetadataMainMenu()
        {
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
            MessageBox.Show("Open");
            //see Prism's the UI Composition Desktop example !!!
        }


        private void OnSaveMetadata(object args) { MessageBox.Show("Save"); }
        private void OnKillMetadata(object args) { MessageBox.Show("Kill"); }
        private void OnUpdateMetadata(object args) { MessageBox.Show("Update"); }
        private void OnShowSettings(object args) { MessageBox.Show("Settings"); }
    }
}
