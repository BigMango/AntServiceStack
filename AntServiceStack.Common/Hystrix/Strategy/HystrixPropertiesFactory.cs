namespace AntServiceStack.Common.Hystrix.Strategy
{
    using System;
    using System.Collections.Concurrent;
    using AntServiceStack.Common.Hystrix.Strategy.Properties;

    public static class HystrixPropertiesFactory
    {
        private static readonly ConcurrentDictionary<string, IHystrixCommandProperties> commandProperties = new ConcurrentDictionary<string, IHystrixCommandProperties>();

        public static IHystrixCommandProperties GetCommandProperties(HystrixCommandKey commandKey, HystrixCommandPropertiesSetter setter)
        {
            if (commandKey == null)
                throw new ArgumentNullException("commandKey");
            //HystrixPropertiesStrategyDefault
            IHystrixPropertiesStrategy strategy = HystrixPlugins.Instance.PropertiesStrategy;
            string cacheKey = strategy.GetCommandPropertiesCacheKey(commandKey, setter);
            if (String.IsNullOrEmpty(cacheKey))
            {
                return strategy.GetCommandProperties(commandKey, setter);
            }
            else
            {
                return commandProperties.GetOrAdd(cacheKey, w =>
                {
                    if (setter == null)
                    {
                        setter = new HystrixCommandPropertiesSetter();
                    }

                    return strategy.GetCommandProperties(commandKey, setter);
                });
            }
        }
    }
}
