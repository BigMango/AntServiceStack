using System;


namespace AntServiceStack.Common.Hystrix.Strategy.Properties
{
    /// <summary>
    /// Interface providing factory methods for properties used by various components of Hystrix.
    /// </summary>
    public interface IHystrixPropertiesStrategy
    {
        IHystrixCommandProperties GetCommandProperties(HystrixCommandKey commandKey, HystrixCommandPropertiesSetter setter);
        string GetCommandPropertiesCacheKey(HystrixCommandKey commandKey, HystrixCommandPropertiesSetter setter);
    }
}
