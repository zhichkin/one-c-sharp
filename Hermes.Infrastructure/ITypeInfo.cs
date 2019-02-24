using System;
using System.Collections.Generic;
using System.Text;

namespace Zhichkin.Hermes.Infrastructure
{
    public interface ITypeInfo
    {
        int Code { set; get; }
        string Name { set; get; }
    }
}
