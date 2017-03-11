using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Hystrix
{
    using System;

    /// <summary>
    /// A group name for a <see cref="HystrixCommand"/>. This is used for grouping together commands such as for reporting, alerting, dashboards or team/library ownership.
    /// By default this will be used to define the <see cref="HystrixThreadPoolKey"/> unless a separate one is defined.
    /// </summary>
    public class HystrixCommandGroupKey : HystrixKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixCommandGroupKey"/> class.
        /// </summary>
        /// <param name="name">The name of the command group key.</param>
        public HystrixCommandGroupKey(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Converts a string to a <see cref="HystrixCommandGroupKey"/> object.
        /// </summary>
        /// <param name="name">The name of the command group key.</param>
        /// <returns>A <see cref="HystrixCommandGroupKey"/> object constructed from the specified name.</returns>
        public static implicit operator HystrixCommandGroupKey(string name)
        {
            return new HystrixCommandGroupKey(name);
        }
    }
}
