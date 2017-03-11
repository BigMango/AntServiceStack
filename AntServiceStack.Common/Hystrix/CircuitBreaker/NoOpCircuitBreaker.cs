using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Hystrix.CircuitBreaker
{
    /// <summary>
    /// An implementation of the circuit breaker that does nothing.
    /// Used if circuit breaker is disabled for a command.
    /// </summary>
    internal class NoOpCircuitBreaker : IHystrixCircuitBreaker
    {
        /// <inheritdoc />
        public bool AllowRequest()
        {
            return true;
        }

        /// <inheritdoc />
        public bool IsOpen()
        {
            return false;
        }

        /// <inheritdoc />
        public void MarkSuccess()
        {
        }
    }
}
