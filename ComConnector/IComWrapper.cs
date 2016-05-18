using System;

namespace Zhichkin
{
    public interface IComWrapper : IDisposable
    {
        object ComObject { get; }
        object Get(string property_name);
        void Set(string property_name, object value);
        object Call(string method_name);
        object Call(string method_name, params object[] args);
        IComWrapper Wrap(object com_object);
        IComWrapper GetAndWrap(string property_name);
        IComWrapper CallAndWrap(string method_name);
        IComWrapper CallAndWrap(string method_name, params object[] args);
    }
}
