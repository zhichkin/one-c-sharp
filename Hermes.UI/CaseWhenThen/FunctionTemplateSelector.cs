using System.Windows;
using System.Windows.Controls;

namespace Zhichkin.Hermes.UI
{
    public sealed class FunctionTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            if (element == null) return null;

            return element.FindResource("CaseWhenThenTemplate") as DataTemplate;
        }
    }
}
