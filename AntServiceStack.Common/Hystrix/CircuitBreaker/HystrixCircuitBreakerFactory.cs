using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Hystrix.CircuitBreaker
{
    using System.Collections.Concurrent;

    /// <summary>
    /// Factory of <see cref="IHystrixCircutBreaker"/> instances.
    /// Thread safe and ensures only 1 <see cref="IHystrixCircuitBreaker"/> per <see cref="HystrixCommandKey"/>.
    /// </summary>
    public static class HystrixCircuitBreakerFactory
    {
        /// <summary>
        /// Stores instances of <see cref="IHystrixCircuitBreaker"/>.
        /// </summary>
        private static readonly ConcurrentDictionary<HystrixCommandKey, IHystrixCircuitBreaker> Instances = new ConcurrentDictionary<HystrixCommandKey, IHystrixCircuitBreaker>();

        /// <summary>
        /// Gets the <see cref="IHystrixCircuitBreaker"/> instance for a given <see cref="HystrixCommandKey"/> or null if none exists.
        /// </summary>
        /// <param name="key">The key of the command requesting the circuit breaker.</param>
        /// <returns>The circuit breaker instance of the command or null if none exists.</returns>
        public static IHystrixCircuitBreaker GetInstance(HystrixCommandKey key)
        {
            return Instances.GetOrDefault(key);
        }

        /// <summary>
        /// Gets the <see cref="IHystrixCircuitBreaker"/> instance for a given <see cref="HystrixCommandKey"/>.
        /// If no circuit breaker exists for the specified command key, a new one will be created using the properties and metrics parameters.
        /// If a circuit breaker already exists, those parameters will be ignored.
        /// </summary>
        /// <param name="commandKey">Command key of command instance requesting the circuit breaker.</param>
        /// <param name="properties">The properties of the specified command.</param>
        /// <param name="metrics">The metrics of the specified command.</param>
        /// <returns>A new or an existing circuit breaker instance.</returns>
        public static IHystrixCircuitBreaker GetInstance(HystrixCommandKey commandKey, IHystrixCommandProperties properties, HystrixCommandMetrics metrics)
        {
            return Instances.GetOrAdd(commandKey, w => new HystrixCircuitBreakerImpl(properties, metrics));
        }

        /// <summary>
        /// Clears all circuit breakers. If new requests come in instances will be recreated.
        /// </summary>
        internal static void Reset()
        {
            Instances.Clear();
        }
    }
}
