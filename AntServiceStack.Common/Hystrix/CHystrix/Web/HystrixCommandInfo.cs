namespace CHystrix.Web
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class HystrixCommandInfo
    {
        [DataMember]
        public long currentConcurrentExecutionCount { get; set; }

        [DataMember]
        public long currentTime { get; set; }

        [DataMember]
        public long errorCount { get; set; }

        [DataMember]
        public long errorPercentage { get; set; }

        [DataMember]
        public string group { get; set; }

        [DataMember]
        public bool isCircuitBreakerOpen { get; set; }

        [DataMember]
        public PercentileInfo latencyExecute { get; set; }

        [DataMember]
        public long latencyExecute_mean { get; set; }

        [DataMember]
        public PercentileInfo latencyTotal { get; set; }

        [DataMember]
        public long latencyTotal_mean { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public bool propertyValue_circuitBreakerEnabled { get; set; }

        [DataMember]
        public int propertyValue_circuitBreakerErrorThresholdPercentage { get; set; }

        [DataMember]
        public bool propertyValue_circuitBreakerForceClosed { get; set; }

        [DataMember]
        public bool propertyValue_circuitBreakerForceOpen { get; set; }

        [DataMember]
        public int propertyValue_circuitBreakerRequestVolumeThreshold { get; set; }

        [DataMember]
        public long propertyValue_circuitBreakerSleepWindowInMilliseconds { get; set; }

        [DataMember]
        public long propertyValue_executionIsolationSemaphoreMaxConcurrentRequests { get; set; }

        [DataMember]
        public string propertyValue_executionIsolationStrategy { get; set; }

        [DataMember]
        public long propertyValue_executionIsolationThreadInterruptOnTimeout { get; set; }

        [DataMember]
        public long propertyValue_executionIsolationThreadPoolKeyOverride { get; set; }

        [DataMember]
        public long propertyValue_executionIsolationThreadTimeoutInMilliseconds { get; set; }

        [DataMember]
        public long propertyValue_fallbackIsolationSemaphoreMaxConcurrentRequests { get; set; }

        [DataMember]
        public long propertyValue_metricsRollingStatisticalWindowInMilliseconds { get; set; }

        [DataMember]
        public bool propertyValue_requestCacheEnabled { get; set; }

        [DataMember]
        public bool propertyValue_requestLogEnabled { get; set; }

        [DataMember]
        public int reportingHosts { get; set; }

        [DataMember]
        public long requestCount { get; set; }

        [DataMember]
        public long rollingCountCollapsedRequests { get; set; }

        [DataMember]
        public long rollingCountExceptionsThrown { get; set; }

        [DataMember]
        public long rollingCountFailure { get; set; }

        [DataMember]
        public long rollingCountFallbackFailure { get; set; }

        [DataMember]
        public long rollingCountFallbackRejection { get; set; }

        [DataMember]
        public long rollingCountFallbackSuccess { get; set; }

        [DataMember]
        public long rollingCountResponsesFromCache { get; set; }

        [DataMember]
        public long rollingCountSemaphoreRejected { get; set; }

        [DataMember]
        public long rollingCountShortCircuited { get; set; }

        [DataMember]
        public long rollingCountSuccess { get; set; }

        [DataMember]
        public long rollingCountThreadPoolRejected { get; set; }

        [DataMember]
        public long rollingCountTimeout { get; set; }

        [DataMember]
        public string type { get; set; }
    }
}

