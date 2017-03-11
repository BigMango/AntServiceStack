using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Hystrix
{
    using AntServiceStack.Common.Hystrix.CircuitBreaker;
    using AntServiceStack.Common.Hystrix.Strategy;

    public class HystrixCommand
    {
        internal protected readonly IHystrixCircuitBreaker circuitBreaker;
        internal protected readonly IHystrixCommandProperties properties;
        internal protected readonly HystrixCommandMetrics metrics;

        /// <summary>
        /// 二级目录 + ActionName
        /// </summary>
        public readonly string CommandKey;

        public HystrixCommand(string servicePath, string opName, string serviceName, string fullServiceName, string metricPrefix/*"soa.service"*/ , HystrixCommandPropertiesSetter commandPropertiesDefaults)
        {
            // op name & service name initialization
            if (opName == null)
                throw new ArgumentNullException("opName");
            if (serviceName == null)
                throw new ArgumentNullException("serviceName");
            if (metricPrefix == null)
                throw new ArgumentNullException("metricPrefix");
            if (fullServiceName == null)
                throw new ArgumentNullException("fullServiceName");

            CommandKey = servicePath + "." + opName;

            // Properties initialization commandPropertiesDefaults 只设置了 电容器的开关和方法的执行timeout 2个参数
            this.properties = HystrixPropertiesFactory.GetCommandProperties(CommandKey, commandPropertiesDefaults);

            // Metrics initializtion
            this.metrics = HystrixCommandMetrics.GetInstance(CommandKey, opName, serviceName, fullServiceName, metricPrefix, this.properties);

            // CircuitBreaker initializtion
            if (this.properties.CircuitBreakerEnabled.Get())
            {
                this.circuitBreaker = HystrixCircuitBreakerFactory.GetInstance(CommandKey, this.properties, this.metrics);
            }
            else
            {
                this.circuitBreaker = new NoOpCircuitBreaker();
            }
        }

        public IHystrixCircuitBreaker CircuitBreaker { get { return this.circuitBreaker; } }

        /**
         * The {@link HystrixCommandMetrics} associated with this {@link HystrixCommand} instance.
         *
         * @return HystrixCommandMetrics
         */
        public HystrixCommandMetrics Metrics { get { return this.metrics; } }

        /**
         * The {@link HystrixCommandProperties} associated with this {@link HystrixCommand} instance.
         *
         * @return HystrixCommandProperties
         */
        public IHystrixCommandProperties Properties { get { return this.properties; } }

    }
}
