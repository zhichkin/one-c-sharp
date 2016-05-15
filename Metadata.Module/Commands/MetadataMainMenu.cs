using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Commands;

namespace Zhichkin.Metadata.Commands
{
    public class OpenMetadataCommand<T> : DelegateCommandBase
    {
        public OpenMetadataCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod) : base((o) => executeMethod((T)o), (o) => canExecuteMethod((T)o)) { }
    }
    public class SaveMetadataCommand<T> : DelegateCommandBase
    {
        public SaveMetadataCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod) : base((o) => executeMethod((T)o), (o) => canExecuteMethod((T)o)) { }
    }
    public class KillMetadataCommand<T> : DelegateCommandBase
    {
        public KillMetadataCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod) : base((o) => executeMethod((T)o), (o) => canExecuteMethod((T)o)) { }
    }
    public class UpdateMetadataCommand<T> : DelegateCommandBase
    {
        public UpdateMetadataCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod) : base((o) => executeMethod((T)o), (o) => canExecuteMethod((T)o)) { }
    }
    public class ShowSettingsCommand<T> : DelegateCommandBase
    {
        public ShowSettingsCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod) : base((o) => executeMethod((T)o), (o) => canExecuteMethod((T)o)) { }
    }
}
