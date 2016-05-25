using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using System.Collections;

namespace Zhichkin.Metadata.Views
{
    public sealed class EntityChildrenConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            List<object> items = new List<object>();

            for (int i = 0; i < values.Length; i++)
            {
                IEnumerable children = (IEnumerable)values[i];

                foreach (var child in children)
                {
                    items.Add(child);
                }
            }

            return items;
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot perform reverse-conversion.");
        }
    }
}
