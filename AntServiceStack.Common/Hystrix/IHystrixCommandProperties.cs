using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Hystrix
{
    public interface IHystrixCommandProperties
    {
        /// <summary>
        /// 是否开启容器的功能 默认true 这个是总开关  如果设置为true 每个方法都不会设置容器保护
        /// </summary>
        IHystrixProperty<bool> CircuitBreakerEnabled { get; }
        /// <summary>
        /// if 50%+ of requests in 10 seconds are failures or latent when we will trip the circuit
        /// 如果大于等于百分之50的请求在10秒内都失败了 会开启自我保护模式
        /// 默认50
        /// </summary>
        IHystrixProperty<int> CircuitBreakerErrorThresholdPercentage { get; }
        /// <summary>
        /// 开砸是联通的状态 默认false 如果设置为true 就代表关闭 SOA服务 容器保护的功能 这也是个子开关
        /// </summary>
        IHystrixDynamicProperty<bool> CircuitBreakerForceClosed { get; }
        /// <summary>
        /// 开闸是断开的状态 默认false
        /// </summary>
        IHystrixDynamicProperty<bool> CircuitBreakerForceOpen { get; }
        /// <summary>
        /// 在10秒内20个请求必须发生在统计信息之前
        /// </summary>
        IHystrixProperty<int> CircuitBreakerRequestVolumeThreshold { get; }

        /// <summary>
        /// 休眠多少时间再跳匝 默认5秒
        /// </summary>
        IHystrixProperty<TimeSpan> CircuitBreakerSleepWindow { get; }
        /// <summary>
        /// 每个方法的执行timeout 默认20秒
        /// </summary>
        IHystrixDynamicProperty<TimeSpan?> ExecutionIsolationThreadTimeout { get; }

        /// <summary>
        /// 50毫秒就checkhealth一次？
        /// </summary>

        IHystrixProperty<TimeSpan> MetricsHealthSnapshotInterval { get; }
        /// <summary>
        ///  默认10秒
        /// </summary>
        IHystrixProperty<int> MetricsRollingStatisticalWindowInMilliseconds { get; }
        /// <summary>
        /// default => statisticalWindowBuckets: 10 = 10 buckets in a 10 seconds
        /// </summary>
        IHystrixProperty<int> MetricsRollingStatisticalWindowBuckets { get; }
        /// <summary>
        /// 默认60秒
        /// </summary>
        IHystrixProperty<int> MetricsIntegerBufferTimeWindowInSeconds { get; }
        /// <summary>
        /// 默认10个桶
        /// </summary>
        IHystrixProperty<int> MetricsIntegerBufferBucketTimeWindowInSeconds { get; }

        /// <summary>
        /// 限制默认为200
        /// </summary>
        IHystrixProperty<int> MetricsIntegerBufferBucketSizeLimit { get; }
        /// <summary>
        /// 默认false
        /// </summary>
        IHystrixProperty<bool> RequestLogEnabled { get; }
    }
}
