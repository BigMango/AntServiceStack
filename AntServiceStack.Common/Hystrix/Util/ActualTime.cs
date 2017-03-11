using System;
using System.Diagnostics;

namespace AntServiceStack.Common.Hystrix.Util
{
    /// <summary>
    /// Provides a method to get the current time. It uses <see cref="Stopwatch"/>.
    /// </summary>
    internal class ActualTime : ITime
    {
        /// <summary>
        /// The singleton instance of the <see cref="ActualTime"/> class.
        /// </summary>
        public static readonly ActualTime Instance = new ActualTime();

        /// <summary>
        /// The instance which provides the accurate time measurement.
        /// </summary>
        private Stopwatch stopwatch;

        /// <summary>
        /// Prevents a default instance of the <see cref="ActualTime"/> class from being created.
        /// </summary>
        private ActualTime()
        {
            this.stopwatch = new Stopwatch();
            this.stopwatch.Start();
        }

        /// <summary>
        /// Gets the current time in milliseconds.
        /// </summary>
        public static long CurrentTimeInMillis
        {
            get
            {
                return Instance.GetCurrentTimeInMillis();
            }
        }

        /// <summary>
        /// Gets the current time in milliseconds.
        /// </summary>
        /// <returns>The current time in milliseconds.</returns>
        public long GetCurrentTimeInMillis()
        {
            return this.stopwatch.ElapsedMilliseconds;
        }
    }
}
