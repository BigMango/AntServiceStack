namespace AntServiceStack.Common.Hystrix.CircuitBreaker
{
    /// <summary>
    /// Circuit-breaker logic that is hooked into <see cref="HystrixCommand"/> execution and will stop allowing executions if failures have gone past the defined threshold.
    /// It will then allow single retries after a defined sleep window until the execution succeeds at which point it will close the circuit and allow executions again.
    /// </summary>
    public interface IHystrixCircuitBreaker
    {
        /// <summary>
        /// Every <see cref="HystrixCommand"/> request asks this if it is allowed to proceed or not.
        /// <p>
        /// This takes into account the half-open logic which allows some requests through when determining if it should be closed again.
        /// </summary>
        /// <returns>True is the request is permitted, otherwise false.</returns>
        bool AllowRequest();

        /// <summary>
        /// Gets whether the circuit is currently open (tripped).
        /// </summary>
        /// <returns>True if the circuit is open, otherwise false.</returns>
        bool IsOpen();

        /// <summary>
        /// Invoked on successful executions from <see cref="HystrixCommand"/> as part of feedback mechanism when in a half-open state.
        /// </summary>
        void MarkSuccess();
    }
}
