using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Hystrix
{
    /// <summary>
    /// Provides an interface to represent property values which are used in
    /// <see cref="AntServiceStack.Common.Hystrix.IHystrixCommandProperties"/>.
    /// This way Hystrix can consume properties
    /// without being tied to any particular backing implementation. The actual implementation
    /// is decided by the current <see cref="IHystrixPropertiesStrategy"/>.
    /// </summary>
    /// <typeparam name="T">The type of property value.</typeparam>
    /// <seealso cref="IHystrixPropertiesStrategy"/>
    /// <seealso cref="IHystrixCommandProperties"/>
    /// <author>William Yang</author>
    public interface IHystrixProperty<T>
    {
        T Get();
    }
}
