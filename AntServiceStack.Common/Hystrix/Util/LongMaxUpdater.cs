using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Hystrix.Util
{
    using AntServiceStack.Common.Hystrix.Atomic;

    /// <summary>
    /// This class is currently uses <see cref="AtomicLong"/> to calculate the maximum of a series of number.
    /// The original implementation derive from <see cref="Striped64"/> to implement a counter with better throughput under high contention.
    /// </summary>
    public class LongMaxUpdater
    {
        /// <summary>
        /// The <see cref="AtomicLong"/> instance storing the maximum.
        /// </summary>
        private readonly AtomicLong value = new AtomicLong(long.MinValue);

        /// <summary>
        /// Updates the maximum if the specified number is greater than the maximum.
        /// </summary>
        /// <param name="x">The number to update with.</param>
        public void Update(long x)
        {
            while (true)
            {
                long current = this.value.Value;
                if (current >= x)
                {
                    return;
                }

                if (this.value.CompareAndSet(current, x))
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Gets the maximum.
        /// </summary>
        /// <returns>The maximum.</returns>
        public long Max()
        {
            return this.value.Value;
        }

        /// <summary>
        /// Sets the maximum to 0.
        /// </summary>
        public void Reset()
        {
            this.value.GetAndSet(0);
        }

        /// <summary>
        /// Gets the maximum and sets it to 0.
        /// </summary>
        /// <returns>The maximum.</returns>
        public long MaxThenReset()
        {
            return this.value.GetAndSet(0);
        }

        /// <summary>
        /// Gets the string representation of the maximum.
        /// </summary>
        /// <returns>The string representation of the maximum.</returns>
        public override string ToString()
        {
            return this.value.ToString();
        }
    }
}
