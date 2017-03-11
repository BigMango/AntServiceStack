using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Hystrix.Util
{
    /// <summary>
    /// <para>
    /// Various states/events that can be captured in the <see cref="HystrixRollingNumber"/>.
    /// </para>
    /// <para>
    /// Events can be type of Counter or MaxUpdater, which can be determined using the
    /// <see cref="HystrixRollingNumberEventExtensions.IsCounter()"/> or
    /// <see cref="HystrixRollingNumberEventExtensions.IsMaxUpdater()"/> extension methods.
    /// </para>
    /// <para>
    /// The Counter type events can be used with <see cref="HystrixRollingNumber.Increment()"/>, <see cref="HystrixRollingNumber.Add()"/>,
    /// <see cref="HystrixRollingNumber.GetRollingSum()"/> methods.
    /// </para>
    /// <para>
    /// The MaxUpdater type events can be used with <see cref="HystrixRollingNumber.UpdateRollingMax()"/> and <see cref="HystrixRollingNumber.GetRollingMax()"/> methods.
    /// </para>
    /// </summary>
    public enum HystrixRollingNumberEvent
    {
        /// <summary>
        /// When a <see cref="HystrixCommand" /> successfully completes.
        /// </summary>
        Success,

        /// <summary>
        /// When a <see cref="HystrixCommand" /> times out (fails to complete).
        /// </summary>
        Timeout,

        /// <summary>
        /// When a <see cref="HystrixCommand" /> performs a short-circuited fallback.
        /// </summary>
        ShortCircuited,

        /// <summary>
        /// When a <see cref="HystrixCommand" /> is unable to queue up (thread pool rejection).
        /// </summary>
        ThreadPoolRejected,

        /// <summary>
        /// When AntServiceStack throws an Framework exception.
        /// </summary>
        FrameworkExceptionThrown,

        /// <summary>
        /// When AntServiceStack throws an Service exception.
        /// </summary>
        ServiceExceptionThrown,

        /// <summary>
        /// When AntServiceStack throws an Validation exception.
        /// </summary>
        ValidationExceptionThrown,
    }
}

