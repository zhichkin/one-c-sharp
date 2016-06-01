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
    public sealed class CompositeCollectionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var list = new CompositeCollection();

            foreach (var item in values)
            {
                if (item is IEnumerable)
                {
                    list.Add(new CollectionContainer()
                        {
                            Collection = (IEnumerable)item
                        });
                }
                else
                {
                    list.Add(item);
                }
            }

            return list;
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
