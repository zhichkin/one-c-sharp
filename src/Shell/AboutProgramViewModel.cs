using Microsoft.Practices.Prism.Mvvm;
using System.Diagnostics;
using System.Reflection;

namespace Zhichkin.Shell
{
    public sealed class AboutProgramViewModel : BindableBase
    {
        public AboutProgramViewModel()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
            _VersionInfo = info.FileVersion;
        }

        private string _VersionInfo;
        public string VersionInfo
        {
            get { return _VersionInfo; }
            set
            {
                _VersionInfo = value;
                this.OnPropertyChanged(nameof(VersionInfo));
            }
        }
    }
}