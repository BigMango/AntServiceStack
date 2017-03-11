namespace AntServiceStack.Common.Hystrix.Strategy.Properties
{
    public class HystrixPropertiesStrategyDefault : IHystrixPropertiesStrategy
    {
        private static readonly HystrixPropertiesStrategyDefault instance = new HystrixPropertiesStrategyDefault();
        public static HystrixPropertiesStrategyDefault Instance { get { return instance; } }

        protected HystrixPropertiesStrategyDefault()
        {
        }

        public virtual IHystrixCommandProperties GetCommandProperties(HystrixCommandKey commandKey, HystrixCommandPropertiesSetter setter)
        {
            return new HystrixPropertiesCommandDefault(setter);
        }
        public virtual string GetCommandPropertiesCacheKey(HystrixCommandKey commandKey, HystrixCommandPropertiesSetter setter)
        {
            return commandKey.Name;
        }
    }
}
