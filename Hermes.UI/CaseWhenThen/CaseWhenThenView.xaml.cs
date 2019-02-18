using System.Windows;
using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public partial class CaseWhenThenView : UserControl
    {
        //public static readonly DependencyProperty MyPropertyProperty;
        //private static void OnMyPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        //{
        //    CaseWhenThenView control = target as CaseWhenThenView;
        //    if (control == null) return;
        //    control.MyProperty = (args.NewValue == null ? null : (string)args.NewValue);
        //}
        //public string MyProperty
        //{
        //    set { SetValue(MyPropertyProperty, value); }
        //    get { return (string)GetValue(MyPropertyProperty); }
        //}
        //static CaseWhenThenView()
        //{
        //    MyPropertyProperty = DependencyProperty.Register("MyProperty", typeof(string), typeof(CaseWhenThenView),
        //        new FrameworkPropertyMetadata(
        //            null,
        //            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
        //            new PropertyChangedCallback(OnMyPropertyChanged)));
        //}
        public CaseWhenThenView()
        {
            CaseWhenThenViewModel viewModel = new CaseWhenThenViewModel()
            {
                Name = "CaseWhenThen Control"
            };
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
