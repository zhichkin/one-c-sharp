using System;
using System.Collections.Generic;
using System.Text;

namespace Zhichkin.Hermes.Infrastructure
{
    public interface IParameter
    {
        string Name { get; set; }
        IEntityInfo Type { get; set; }
        object Value { get; set; }
    }
}
