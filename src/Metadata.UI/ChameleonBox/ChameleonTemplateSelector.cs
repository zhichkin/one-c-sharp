using System;
using System.Windows;
using System.Windows.Controls;
using Zhichkin.ORM;
using Zhichkin.Shell;

namespace Zhichkin.Metadata.UI
{
    public sealed class ChameleonTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            if (element == null) return null;

            DataGridCell cell = element.GetParent<DataGridCell>();
            bool isEditing = (cell == null || cell.IsEditing);

            ChameleonBox box = item as ChameleonBox;
            if (box == null) return null;

            Type type = box.ChameleonType?.GetCLRType();
                        
            string templateName = isEditing ?
                GetEditableTemplateName(type) :
                GetReadOnlyTemplateName(type);

            return element.FindResource(templateName) as DataTemplate;
        }
        private string GetReadOnlyTemplateName(Type type)
        {
            if (type == typeof(bool)) { return "BooleanReadOnlyTemplate"; }
            else if (type == typeof(int)) { return "Int32ReadOnlyTemplate"; }
            else if (type == typeof(decimal)) { return "DecimalReadOnlyTemplate"; }
            else if (type == typeof(DateTime)) { return "DateTimeReadOnlyTemplate"; }
            else if (type == typeof(string)) { return "StringReadOnlyTemplate"; }
            else if (type == typeof(object)) { return "ReferenceObjectReadOnlyTemplate"; }
            return "EmptyReadOnlyTemplate";
        }
        private string GetEditableTemplateName(Type type)
        {
            if (type == typeof(bool)) { return "BooleanEditableTemplate"; }
            else if (type == typeof(int)) { return "Int32EditableTemplate"; }
            else if (type == typeof(decimal)) { return "DecimalEditableTemplate"; }
            else if (type == typeof(DateTime)) { return "DateTimeEditableTemplate"; }
            else if (type == typeof(string)) { return "StringEditableTemplate"; }
            else if (type == typeof(object)) { return "ReferenceObjectEditableTemplate"; }
            return "EmptyEditableTemplate";
        }
    }
}
