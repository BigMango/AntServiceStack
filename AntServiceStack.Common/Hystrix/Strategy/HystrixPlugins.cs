namespace AntServiceStack.Common.Hystrix.Strategy
{
    using System;
    using System.Configuration;
    using System.Linq;
    using AntServiceStack.Common.Hystrix.Atomic;
    using AntServiceStack.Common.Hystrix.Strategy.Properties;
    using AntServiceStack.Common.Hystrix.Strategy.Metrics;

    public class HystrixPlugins
    {
        private static readonly HystrixPlugins instance = new HystrixPlugins();
        public static HystrixPlugins Instance { get { return instance; } }

        private readonly AtomicReference<IHystrixPropertiesStrategy> propertyStrategy = new AtomicReference<IHystrixPropertiesStrategy>();
        private readonly AtomicReference<IHystrixMetricsPublisher> metricsPublisher = new AtomicReference<IHystrixMetricsPublisher>();

        private HystrixPlugins()
        {
        }
        public IHystrixPropertiesStrategy PropertiesStrategy
        {
            get
            {
                if (this.propertyStrategy.Value == null)
                {
                    this.propertyStrategy.CompareAndSet(null, GetPluginImplementationViaConfiguration<IHystrixPropertiesStrategy>() ?? HystrixPropertiesStrategyDefault.Instance);
                }

                return this.propertyStrategy.Value;
            }
        }

        public IHystrixMetricsPublisher MetricsPublisher
        {
            get
            {
                if (this.metricsPublisher.Value == null)
                {
                    this.metricsPublisher.CompareAndSet(null, GetPluginImplementationViaConfiguration<IHystrixMetricsPublisher>() ?? HystrixMetricsPublisherDefault.Instance);
                }

                return this.metricsPublisher.Value;
            }
        }

        public void RegisterPropertiesStrategy(IHystrixPropertiesStrategy implementation)
        {
            if (!this.propertyStrategy.CompareAndSet(null, implementation))
            {
                throw new InvalidOperationException("Another strategy was alread registered.");
            }
        }

        private static T GetPluginImplementationViaConfiguration<T>()
        {
            return (T)GetPluginImplementationViaConfiguration(typeof(T));
        }

        private static object GetPluginImplementationViaConfiguration(Type pluginType)
        {
            string pluginTypeName = pluginType.Name;
            string implementationTypeName = ConfigurationManager.AppSettings["Ant.SOA.AntServiceStack.Common.Hystrix.Plugin." + pluginTypeName + ".Implementation"];
            if (String.IsNullOrEmpty(implementationTypeName))
                return null;

            Type implementationType;
            try
            {
                implementationType = Type.GetType(implementationTypeName, true);
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("{0} implementation class not found: {1}", pluginType, implementationTypeName), e);
            }

            if (!implementationType.IsSubclassOf(pluginType) && !implementationType.GetInterfaces().Contains(pluginType))
            {
                throw new Exception(String.Format("{0} implementation is not an instance of {0}: {1}", pluginTypeName, implementationTypeName));
            }

            try
            {
                return Activator.CreateInstance(implementationType);
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("{0} implementation not able to be instantiated: {1}", pluginType, implementationTypeName), e);
            }
        }
    }
}
