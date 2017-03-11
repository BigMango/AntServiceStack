using AntServiceStack.Common.Hystrix.CircuitBreaker;

namespace AntServiceStack.Common.Hystrix.Strategy.Metrics
{
    public class HystrixMetricsPublisherCommandDefault : IHystrixMetricsPublisherCommand
    {
        public HystrixMetricsPublisherCommandDefault(HystrixCommandKey commandKey, HystrixCommandGroupKey commandGroupKey, HystrixCommandMetrics metrics, IHystrixCircuitBreaker circuitBreaker, IHystrixCommandProperties properties)
        {
            // do nothing by default
        }

        public void Initialize()
        {
            // do nothing by default
        }
    }
}
