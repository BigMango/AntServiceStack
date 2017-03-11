namespace AntServiceStack.Common.Hystrix.Util
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// Provides helper methods for the <see cref="HystrixRollingNumberEvent"/> enumeration.
    /// </summary>
    public static class HystrixRollingNumberEventExtensions
    {
        /// <summary>
        /// All possible values of the <see cref="HystrixRollingNumberEvent"/> enumeration.
        /// </summary>
        public static readonly ReadOnlyCollection<HystrixRollingNumberEvent> Values = Enum.GetValues(typeof(HystrixRollingNumberEvent)).Cast<HystrixRollingNumberEvent>().ToList().AsReadOnly();

        /// <summary>
        /// Gets whether the specified event is a Counter type or not.
        /// </summary>
        /// <param name="rollingNumberEvent">The specified event.</param>
        /// <returns>True if it's a Counter type, otherwise false.</returns>
        public static bool IsCounter(this HystrixRollingNumberEvent rollingNumberEvent)
        {
            return !rollingNumberEvent.IsMaxUpdater();
        }

        /// <summary>
        /// Gets whether the specified event is a MaxUpdater type or not.
        /// </summary>
        /// <param name="rollingNumberEvent">The specified event.</param>
        /// <returns>True if it's a MaxUpdater type, otherwise false.</returns>
        public static bool IsMaxUpdater(this HystrixRollingNumberEvent rollingNumberEvent)
        {
            return false;
        }
    }
}
