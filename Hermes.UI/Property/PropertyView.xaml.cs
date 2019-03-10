using System.Windows;
using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public partial class PropertyView : UserControl
    {
        //public static readonly DependencyProperty TableProperty;
        //public TableExpression Table
        //{
        //    set { SetValue(TableProperty, value); }
        //    get { return (TableExpression)GetValue(TableProperty); }
        //}
        //static PropertyView()
        //{
        //    TableProperty = DependencyProperty.Register(
        //        "Table", typeof(TableExpression), typeof(PropertyExpression),
        //        new FrameworkPropertyMetadata(
        //            null,
        //            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
        //            new PropertyChangedCallback(OnTableChanged)));
        //}
        //private static void OnTableChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        //{
        //    PropertyView control = target as PropertyView;
        //    if (control == null) return;
        //    control.Table = (args.NewValue == null ? null : (TableExpression)args.NewValue);
        //}

        public PropertyView()
        {
            InitializeComponent();
        }
    }
}
