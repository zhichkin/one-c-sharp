using System;
using System.Collections.Generic;
using System.Text;

namespace Zhichkin.Hermes.Infrastructure
{
    public interface IEntityInfo
    {
        int TypeCode { get; }
        string Name { get; set; }
    }
}
