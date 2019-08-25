using System;
using System.Windows.Controls;
using Zhichkin.Metadata.ViewModels;

namespace Zhichkin.Metadata.Views
{
    public partial class SQLServerConnectionView : UserControl
    {
        public SQLServerConnectionView()
        {
            this.DataContext = new SQLConnectionDialogViewModel();
            InitializeComponent();
        }
    }
}
