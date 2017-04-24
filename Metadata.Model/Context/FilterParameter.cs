using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zhichkin.Metadata.Model
{
    public sealed class FilterParameter
    {
        public string Name { get; set; }
        public FilterOperator Operator { get; set; }
        public object Value { get; set; }
    }
}
