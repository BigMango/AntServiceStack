using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Hystrix.Strategy.Metrics
{
    using AntServiceStack.Common.Hystrix.CircuitBreaker;

    public class HystrixMetricsPublisherDefault : IHystrixMetricsPublisher
    {
        private static HystrixMetricsPublisherDefault instance = new HystrixMetricsPublisherDefault();
        public static HystrixMetricsPublisherDefault Instance { get { return instance; } }

        protected HystrixMetricsPublisherDefault()
        {
        }

        public virtual IHystrixMetricsPublisherCommand GetMetricsPublisherForCommand(HystrixCommandKey commandKey, HystrixCommandGroupKey commandGroupKey, HystrixCommandMetrics metrics, IHystrixCircuitBreaker circuitBreaker, IHystrixCommandProperties properties)
        {
            return new HystrixMetricsPublisherCommandDefault(commandKey, commandGroupKey, metrics, circuitBreaker, properties);
        }
    }
}
