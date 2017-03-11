using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Hystrix.Util
{
    /// <summary>
    /// Provides a method to get the current time.
    /// </summary>
    internal interface ITime
    {
        /// <summary>
        /// Gets the current time in milliseconds.
        /// </summary>
        /// <returns>The current time in milliseconds.</returns>
        long GetCurrentTimeInMillis();
    }
}
