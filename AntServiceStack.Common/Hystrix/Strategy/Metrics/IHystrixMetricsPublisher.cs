using AntServiceStack.Common.Hystrix.CircuitBreaker;

namespace AntServiceStack.Common.Hystrix.Strategy.Metrics
{
    public interface IHystrixMetricsPublisher
    {
        IHystrixMetricsPublisherCommand GetMetricsPublisherForCommand(HystrixCommandKey commandKey, HystrixCommandGroupKey commandGroupKey, HystrixCommandMetrics metrics, IHystrixCircuitBreaker circuitBreaker, IHystrixCommandProperties properties);
    }
}
