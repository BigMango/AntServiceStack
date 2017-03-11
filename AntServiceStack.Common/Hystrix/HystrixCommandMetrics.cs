namespace AntServiceStack.Common.Hystrix
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using AntServiceStack.Common.Hystrix.Strategy;
    using AntServiceStack.Common.Hystrix.Util;
    using Freeway.Logging;
    using AntServiceStack.Common.Hystrix.Atomic;

    public class HystrixCommandMetrics
    {
        private static readonly ConcurrentDictionary<string, HystrixCommandMetrics> metrics = new ConcurrentDictionary<string, HystrixCommandMetrics>();

        public static HystrixCommandMetrics GetInstance(string commandKey, string opName, string serviceName, string fullServiceName, string metricPrefix, IHystrixCommandProperties properties)
        {
            return metrics.GetOrAdd(commandKey, key => new HystrixCommandMetrics(opName, serviceName, fullServiceName, metricPrefix, properties));
        }

        internal static void Reset()
        {
            metrics.Clear();
        }

        private readonly IHystrixCommandProperties properties;
        private readonly HystrixRollingNumber counter;
        private readonly string opName;
        private readonly string serviceName;
        private readonly string fullServiceName;
        private AtomicInteger concurrentExecutionCount = new AtomicInteger(0);
        private AtomicInteger maxConcurrentExecutionCount = new AtomicInteger(0);


        private readonly LongAdder successCounter;
        private readonly LongAdder serviceErrorCounter;
        private readonly LongAdder frameworkErrorCounter;
        private readonly LongAdder validationErrorCounter;
        private readonly LongAdder timeoutCounter;
        private readonly LongAdder shortCircuitCounter;
        private readonly LongAdder threadPoolRejectedCounter;

        private readonly HystrixIntegerCircularBuffer executionOperationLatencyBuffer;
        private readonly HystrixIntegerCircularBuffer requestLatencyBuffer;

        public string OperationName { get { return this.opName; } }
        public string ServiceName { get { return this.serviceName; } }
        public string FullServiceName { get { return this.fullServiceName; } }
        public IHystrixCommandProperties Properties { get { return this.properties; } }
        public int CurrentConcurrentExecutionCount { get { return this.concurrentExecutionCount.Value; } }
        internal int MaxConcurrentExecutionCount { get { return this.maxConcurrentExecutionCount.Value; } }

        public string MetricNameRequestCount { get; internal set; }
        public string MetricNameEventDistribution { get; internal set; }
        public string MetricNameLatency { get; internal set; }
        public string MetricNameLatencyDistribution { get; internal set; }
        public string MetricNameLatencyPercentile { get; internal set; }
        public string MetricNameConcurrentExecutionCount { get; internal set; }

        internal HystrixCommandMetrics(string opName, string serviceName, string fullServiceName, string metricPrefix, IHystrixCommandProperties properties)
        {
            this.serviceName = serviceName;
            this.opName = opName;
            this.properties = properties;
            this.fullServiceName = fullServiceName;
            this.counter = new HystrixRollingNumber(properties.MetricsRollingStatisticalWindowInMilliseconds, properties.MetricsRollingStatisticalWindowBuckets);

            this.successCounter = new LongAdder();
            this.frameworkErrorCounter = new LongAdder();
            this.validationErrorCounter = new LongAdder();
            this.serviceErrorCounter = new LongAdder();
            this.shortCircuitCounter = new LongAdder();
            this.threadPoolRejectedCounter = new LongAdder();
            this.timeoutCounter = new LongAdder();

            int timeWindowInSeconds = properties.MetricsIntegerBufferTimeWindowInSeconds.Get();
            int bucketTimeWindowInSeconds = properties.MetricsIntegerBufferBucketTimeWindowInSeconds.Get();
            int bucketSizeLimit = properties.MetricsIntegerBufferBucketSizeLimit.Get();
            this.executionOperationLatencyBuffer = new HystrixIntegerCircularBuffer(timeWindowInSeconds, bucketTimeWindowInSeconds, bucketSizeLimit);
            this.requestLatencyBuffer = new HystrixIntegerCircularBuffer(timeWindowInSeconds, bucketTimeWindowInSeconds, bucketSizeLimit);

            MetricNameEventDistribution = metricPrefix + ".event.distribution";
            MetricNameConcurrentExecutionCount = metricPrefix + ".execution.concurrency";
            MetricNameRequestCount = metricPrefix + ".request.count";
            MetricNameLatency = metricPrefix + ".request.latency";
            MetricNameLatencyDistribution = metricPrefix + ".request.latency.distribution";
            MetricNameLatencyPercentile = metricPrefix + ".request.latency.percentile";
        }

        public long GetCumulativeCount(HystrixRollingNumberEvent ev)
        {
            return this.counter.GetCumulativeSum(ev);
        }
        public long GetRollingCount(HystrixRollingNumberEvent ev)
        {
            return this.counter.GetRollingSum(ev);
        }
        public long GetServiceExecutionTimePercentile(double percentile)
        {
            return this.executionOperationLatencyBuffer.GetPercentile(percentile);
        }
        public long GetServiceExecutionTimeMean()
        {
            return this.executionOperationLatencyBuffer.GetAuditDataAvg();
        }
        public void GetServiceExecutionTimeMetricsData(out int count, out long sum, out long min, out long max)
        {
            this.executionOperationLatencyBuffer.GetAuditData(out count, out sum, out min, out max);
        }
        public int GetServiceExecutionCountInTimeRange(long low, long? high = null)
        {
            return this.executionOperationLatencyBuffer.GetItemCountInRange(low, high);
        }
        public long GetTotalTimePercentile(double percentile)
        {
            return this.requestLatencyBuffer.GetPercentile(percentile);
        }
        public long GetTotalTimeMean()
        {
            return this.requestLatencyBuffer.GetAuditDataAvg();
        }
        public void GetTotalTimeMetricsData(out int count, out long sum, out long min, out long max)
        {
            this.requestLatencyBuffer.GetAuditData(out count, out sum, out min, out max);
        }
        public int GetServiceExecutionCountInTotalTimeRange(long low, long? high = null)
        {
            return this.requestLatencyBuffer.GetItemCountInRange(low, high);
        }

        internal void ResetCounter()
        {
            this.counter.Reset();
        }

        public long GetSuccessCount()
        {
            return this.successCounter.Sum();
        }

        public long GetServiceErrorCount()
        {
            return this.serviceErrorCounter.Sum();
        }

        public long GetFrameworkErrorCount()
        {
            return this.frameworkErrorCounter.Sum();
        }

        public long GetValidationErrorCount()
        {
            return this.validationErrorCounter.Sum();
        }

        public long GetShortCircuitCount()
        {
            return this.shortCircuitCounter.Sum();
        }

        public long GetThreadPoolRejectedCount()
        {
            return this.threadPoolRejectedCounter.Sum();
        }

        public long GetTimeoutCount()
        {
            return this.timeoutCounter.Sum();
        }

        public void ResetMetricsCounters()
        {
            this.successCounter.Reset();
            this.serviceErrorCounter.Reset();
            this.frameworkErrorCounter.Reset();
            this.validationErrorCounter.Reset();
            this.shortCircuitCounter.Reset();
            this.threadPoolRejectedCounter.Reset();
            this.timeoutCounter.Reset();
        }

        /// <summary>
        /// 请求成功
        /// </summary>
        /// <param name="duration"></param>
        public void MarkSuccess(long duration)
        {
            this.counter.Increment(HystrixRollingNumberEvent.Success);
            this.successCounter.Increment();
        }

        /// <summary>
        /// 请求timeout
        /// </summary>
        /// <param name="duration"></param>
        public void MarkTimeout(long duration)
        {
            this.counter.Increment(HystrixRollingNumberEvent.Timeout);
            this.timeoutCounter.Increment();
        }

        /// <summary>
        /// 短路
        /// </summary>
        public void MarkShortCircuited()
        {
            this.counter.Increment(HystrixRollingNumberEvent.ShortCircuited);
            this.shortCircuitCounter.Increment();
        }

        /// <summary>
        /// 超出最大并发
        /// </summary>
        public void MarkThreadPoolRejection()
        {
            this.counter.Increment(HystrixRollingNumberEvent.ThreadPoolRejected);
            this.threadPoolRejectedCounter.Increment();
        }

        /// <summary>
        /// .net框架报错
        /// </summary>
        public void MarkFrameworkExceptionThrown()
        {
            this.counter.Increment(HystrixRollingNumberEvent.FrameworkExceptionThrown);
            this.frameworkErrorCounter.Increment();
        }

        /// <summary>
        /// 服务接口报错
        /// </summary>
        public void MarkServiceExceptionThrown()
        {
            this.counter.Increment(HystrixRollingNumberEvent.ServiceExceptionThrown);
            this.serviceErrorCounter.Increment();
        }

        /// <summary>
        /// 
        /// </summary>
        public void MarkValidationExceptionThrown()
        {
            this.counter.Increment(HystrixRollingNumberEvent.ValidationExceptionThrown);
            this.validationErrorCounter.Increment();
        }

        public void IncrementConcurrentExecutionCount()
        {
            var operationCount = concurrentExecutionCount.IncrementAndGet();
            if (operationCount > maxConcurrentExecutionCount.Value)
                maxConcurrentExecutionCount.GetAndSet(operationCount);
        }
        public void DecrementConcurrentExecutionCount()
        {
            concurrentExecutionCount.DecrementAndGet();
        }

        public void AddServiceExecutionTime(long duration)
        {
            this.executionOperationLatencyBuffer.Add(duration);
        }
        public void AddTotalExecutionTime(long duration)
        {
            this.requestLatencyBuffer.Add(duration);
        }

        private volatile HealthCounts healthCountsSnapshot = new HealthCounts();
        private long lastHealthCountsSnapshot = ActualTime.CurrentTimeInMillis;

        public HealthCounts GetHealthCounts()
        {
            // we put an interval between snapshots so high-volume commands don't 
            // spend too much unnecessary time calculating metrics in very small time periods
            long lastTime = this.lastHealthCountsSnapshot;
            long currentTime = ActualTime.CurrentTimeInMillis;
            if (currentTime - lastTime >= this.properties.MetricsHealthSnapshotInterval.Get().TotalMilliseconds || this.healthCountsSnapshot == null)
            {
                if (Interlocked.CompareExchange(ref this.lastHealthCountsSnapshot, currentTime, lastTime) == lastTime)
                {
                    // our thread won setting the snapshot time so we will proceed with generating a new snapshot
                    // losing threads will continue using the old snapshot
                    long success = counter.GetRollingSum(HystrixRollingNumberEvent.Success);
                    long timeout = counter.GetRollingSum(HystrixRollingNumberEvent.Timeout); // fallbacks occur on this
                    long threadPoolRejected = counter.GetRollingSum(HystrixRollingNumberEvent.ThreadPoolRejected); // fallbacks occur on this
                    long shortCircuited = counter.GetRollingSum(HystrixRollingNumberEvent.ShortCircuited); // fallbacks occur on this
                    long frameworkException = counter.GetRollingSum(HystrixRollingNumberEvent.FrameworkExceptionThrown);
                    long serviceException = counter.GetRollingSum(HystrixRollingNumberEvent.ServiceExceptionThrown);
                    long validationException = counter.GetRollingSum(HystrixRollingNumberEvent.ValidationExceptionThrown);

                    healthCountsSnapshot = new HealthCounts(success, timeout, threadPoolRejected, shortCircuited,
                        frameworkException, serviceException, validationException);
                }
            }
            return healthCountsSnapshot;
        }
    }
}
