using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Hystrix
{
    /// <summary>
    /// Hystrix property which can be changed dynamically at runtime
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHystrixDynamicProperty<T> : IHystrixProperty<T>
    {
        void Set(T value);
    }
}
