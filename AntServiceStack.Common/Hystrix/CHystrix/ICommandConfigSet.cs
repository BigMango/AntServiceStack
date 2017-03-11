namespace CHystrix
{
    using System;

    public interface ICommandConfigSet
    {
        bool CircuitBreakerEnabled { get; }

        int CircuitBreakerErrorThresholdPercentage { get; set; }

        bool CircuitBreakerForceClosed { get; set; }

        bool CircuitBreakerForceOpen { get; set; }

        int CircuitBreakerRequestCountThreshold { get; set; }

        int CircuitBreakerSleepWindowInMilliseconds { get; }

        int CommandMaxConcurrentCount { get; set; }

        int CommandTimeoutInMilliseconds { get; set; }

        bool DegradeLogLevel { get; set; }

        int FallbackMaxConcurrentCount { get; set; }

        bool LogExecutionError { get; set; }

        int MaxAsyncCommandExceedPercentage { get; set; }

        int MetricsHealthSnapshotIntervalInMilliseconds { get; }

        int MetricsRollingPercentileBucketSize { get; }

        bool MetricsRollingPercentileEnabled { get; }

        int MetricsRollingPercentileWindowBuckets { get; }

        int MetricsRollingPercentileWindowInMilliseconds { get; }

        int MetricsRollingStatisticalWindowBuckets { get; }

        int MetricsRollingStatisticalWindowInMilliseconds { get; }
    }
}

