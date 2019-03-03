using Microsoft.Practices.Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Zhichkin.Hermes.Infrastructure;

namespace Zhichkin.Hermes.UI
{
    public static class HermesUI
    {
        public static ObservableCollection<PropertyExpression> GetTestEntityFields(TableExpression owner)
        {
            List<PropertyExpression> list = new List<PropertyExpression>();
            list.Add(new PropertyExpression(owner) { Name = "Property_0", Alias = "P0" });
            list.Add(new PropertyExpression(owner) { Name = "Property_1", Alias = "P1" });
            list.Add(new PropertyExpression(owner) { Name = "Property_2", Alias = "P2" });
            list.Add(new PropertyExpression(owner) { Name = "Property_3", Alias = "P3" });
            return new ObservableCollection<PropertyExpression>(list);
        }
        public static ObservableCollection<FunctionExpression> GetAvailablePropertiesForSelection(TableExpression table)
        {
            List<FunctionExpression> list = new List<FunctionExpression>();

            foreach (PropertyExpression p in table.Fields)
            {
                list.Add(p);
            }

            return new ObservableCollection<FunctionExpression>(list);
        }
    }
}
