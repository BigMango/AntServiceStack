namespace AntServiceStack.Common.Hystrix.Strategy
{
    using System.Collections.Concurrent;
    using AntServiceStack.Common.Hystrix.CircuitBreaker;
    using AntServiceStack.Common.Hystrix.Strategy.Metrics;

    /// <summary>
    /// Factory for constructing metrics publisher implementations using a <see cref="IHystrixMetricsPublisher"/> implementation provided by <see cref="HystrixPlugins"/>.
    /// </summary>
    public class HystrixMetricsPublisherFactory
    {
        private static readonly HystrixMetricsPublisherFactory instance = new HystrixMetricsPublisherFactory();


        public static IHystrixMetricsPublisherCommand CreateOrRetrievePublisherForCommand(HystrixCommandKey commandKey, HystrixCommandGroupKey commandOwner, HystrixCommandMetrics metrics, IHystrixCircuitBreaker circuitBreaker, IHystrixCommandProperties properties)
        {
            return instance.GetPublisherForCommand(commandKey, commandOwner, metrics, circuitBreaker, properties);
        }

        private readonly IHystrixMetricsPublisher strategy;

        internal HystrixMetricsPublisherFactory()
            : this(HystrixPlugins.Instance.MetricsPublisher)
        {
        }
        internal HystrixMetricsPublisherFactory(IHystrixMetricsPublisher strategy)
        {
            this.strategy = strategy;
        }

        private readonly ConcurrentDictionary<string, IHystrixMetricsPublisherCommand> commandPublishers = new ConcurrentDictionary<string, IHystrixMetricsPublisherCommand>();
        public IHystrixMetricsPublisherCommand GetPublisherForCommand(HystrixCommandKey commandKey, HystrixCommandGroupKey commandOwner, HystrixCommandMetrics metrics, IHystrixCircuitBreaker circuitBreaker, IHystrixCommandProperties properties)
        {
            return this.commandPublishers.GetOrAdd(commandKey.Name,
                w => this.strategy.GetMetricsPublisherForCommand(commandKey, commandOwner, metrics, circuitBreaker, properties),
                w => w.Initialize());
        }
    }
}
