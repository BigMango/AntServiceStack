namespace CHystrix
{
    using CHystrix.CircuitBreaker;
    using CHystrix.Config;
    using CHystrix.Log;
    using CHystrix.Metrics;
    using System;
    using System.Configuration;
    using System.Runtime.CompilerServices;

    internal static class ComponentFactory
    {
        public const string CircuitBreakerErrorThresholdPercentageSettingKey = "CHystrix.CircuitBreakerErrorThresholdPercentage";
        public const string CircuitBreakerForceClosedSettingKey = "CHystrix.CircuitBreakerForceClosed";
        public const string CircuitBreakerRequestCountThresholdSettingKey = "CHystrix.CircuitBreakerRequestCountThreshold";
        public const string CommandTimeoutInMillisecondsSettingKey = "CHystrix.CommandTimeoutInMilliseconds";
        public static readonly int? DefaultCircuitBreakerErrorThresholdPercentage;
        public static readonly bool? DefaultCircuitBreakerForceClosed;
        public static readonly int? DefaultCircuitBreakerRequestCountThreshold;
        public static readonly int? DefaultCommandTimeoutInMilliseconds;
        public static readonly bool DefaultLogExecutionError;
        public static readonly int DefaultMaxAsyncCommandExceedPercentage;
        public static readonly int? DefaultSemaphoreIsolationMaxConcurrentCount;
        public static readonly int? DefaultThreadIsolationMaxConcurrentCount;
        public const int FrameworkDefaultCircuitBreakerErrorThresholdPercentage = 50;
        public const bool FrameworkDefaultCircuitBreakerForceClosed = false;
        public const int FrameworkDefaultCircuitBreakerRequestCountThreshold = 20;
        public const int FrameworkDefaultCommandTimeoutInMilliseconds = 0x7530;
        public const bool FrameworkDefaultDegradeLogLevel = false;
        public const bool FrameworkDefaultLogExecutionError = false;
        public const int FrameworkDefaultMaxAsyncCommandExceedPercentage = 50;
        public const int FrameworkDefaultSemaphoreIsolationMaxConcurrentCount = 100;
        public const int FrameworkDefaultThreadIsolationMaxConcurrentCount = 20;
        public const string LogExecutionErrorSettingKey = "CHystrix.LogExecutionError";
        public const string MaxAsyncCommandExceedPercentageSettingKey = "CHystrix.MaxAsyncCommandExceedPercentage";
        public const int MinGlobalDefaultCircuitBreakerErrorThresholdPercentage = 20;
        public const int MinGlobalDefaultCircuitBreakerRequestCountThreshold = 10;
        public const int MinGlobalDefaultCommandMaxConcurrentCount = 50;
        public const int MinGlobalDefaultCommandTimeoutInMilliseconds = 0x1388;
        public const int MinGlobalDefaultFallbackMaxConcurrentCount = 50;
        public const string SemaphoreIsolationMaxConcurrentCountSettingKey = "CHystrix.SemaphoreIsolationMaxConcurrentCount";
        public const string ThreadIsolationMaxConcurrentCountSettingKey = "CHystrix.ThreadIsolationMaxConcurrentCount";

        static ComponentFactory()
        {
            int num;
            int num2;
            bool flag;
            int num3;
            int num4;
            int num5;
            int.TryParse(ConfigurationManager.AppSettings["CHystrix.CircuitBreakerRequestCountThreshold"], out num);
            if (num > 0)
            {
                DefaultCircuitBreakerRequestCountThreshold = new int?(num);
            }
            int.TryParse(ConfigurationManager.AppSettings["CHystrix.CircuitBreakerErrorThresholdPercentage"], out num2);
            if ((num2 > 0) && (num2 <= 100))
            {
                DefaultCircuitBreakerErrorThresholdPercentage = new int?(num2);
            }
            if (bool.TryParse(ConfigurationManager.AppSettings["CHystrix.CircuitBreakerForceClosed"], out flag))
            {
                DefaultCircuitBreakerForceClosed = new bool?(flag);
            }
            int.TryParse(ConfigurationManager.AppSettings["CHystrix.SemaphoreIsolationMaxConcurrentCount"], out num3);
            if (num3 > 0)
            {
                DefaultSemaphoreIsolationMaxConcurrentCount = new int?(num3);
            }
            int.TryParse(ConfigurationManager.AppSettings["CHystrix.ThreadIsolationMaxConcurrentCount"], out num4);
            if (num4 > 0)
            {
                DefaultThreadIsolationMaxConcurrentCount = new int?(num4);
            }
            int.TryParse(ConfigurationManager.AppSettings["CHystrix.CommandTimeoutInMilliseconds"], out num5);
            if (num5 > 0)
            {
                DefaultCommandTimeoutInMilliseconds = new int?(num5);
            }
            int.TryParse(ConfigurationManager.AppSettings["CHystrix.MaxAsyncCommandExceedPercentage"], out DefaultMaxAsyncCommandExceedPercentage);
            if ((DefaultMaxAsyncCommandExceedPercentage <= 0) || (DefaultMaxAsyncCommandExceedPercentage > 100))
            {
                DefaultMaxAsyncCommandExceedPercentage = 50;
            }
            if (!bool.TryParse(ConfigurationManager.AppSettings["CHystrix.LogExecutionError"], out DefaultLogExecutionError))
            {
                DefaultLogExecutionError = false;
            }
        }

        public static ICircuitBreaker CreateCircuitBreaker(ICommandConfigSet configSet, ICommandMetrics metrics)
        {
            return new CHystrix.CircuitBreaker.CircuitBreaker(configSet, metrics);
        }

        public static ICommandConfigSet CreateCommandConfigSet(IsolationModeEnum isolationMode)
        {
            CommandConfigSet set = new CommandConfigSet {
                IsolationMode = isolationMode,
                CircuitBreakerEnabled = true,
                CircuitBreakerForceOpen = false,
                CircuitBreakerSleepWindowInMilliseconds = 0x1388,
                MetricsRollingStatisticalWindowBuckets = 10,
                MetricsRollingStatisticalWindowInMilliseconds = 0x2710,
                MetricsRollingPercentileEnabled = true,
                MetricsRollingPercentileWindowInMilliseconds = 0xea60,
                MetricsRollingPercentileWindowBuckets = 6,
                MetricsRollingPercentileBucketSize = 100,
                MetricsHealthSnapshotIntervalInMilliseconds = 100,
                MaxAsyncCommandExceedPercentage = DefaultMaxAsyncCommandExceedPercentage,
                DegradeLogLevel = false,
                LogExecutionError = DefaultLogExecutionError
            };
            if (DefaultCircuitBreakerErrorThresholdPercentage.HasValue)
            {
                set.CircuitBreakerErrorThresholdPercentage = DefaultCircuitBreakerErrorThresholdPercentage.Value;
            }
            if (DefaultCircuitBreakerForceClosed.HasValue)
            {
                set.CircuitBreakerForceClosed = DefaultCircuitBreakerForceClosed.Value;
            }
            if (DefaultCircuitBreakerRequestCountThreshold.HasValue)
            {
                set.CircuitBreakerRequestCountThreshold = DefaultCircuitBreakerRequestCountThreshold.Value;
            }
            if (DefaultCommandTimeoutInMilliseconds.HasValue)
            {
                set.CommandTimeoutInMilliseconds = DefaultCommandTimeoutInMilliseconds.Value;
            }
            if ((isolationMode == IsolationModeEnum.SemaphoreIsolation) && DefaultSemaphoreIsolationMaxConcurrentCount.HasValue)
            {
                set.CommandMaxConcurrentCount = DefaultSemaphoreIsolationMaxConcurrentCount.Value;
                set.FallbackMaxConcurrentCount = set.CommandMaxConcurrentCount;
                return set;
            }
            if ((isolationMode == IsolationModeEnum.ThreadIsolation) && DefaultThreadIsolationMaxConcurrentCount.HasValue)
            {
                set.CommandMaxConcurrentCount = DefaultThreadIsolationMaxConcurrentCount.Value;
                set.FallbackMaxConcurrentCount = set.CommandMaxConcurrentCount;
            }
            return set;
        }

        public static ICommandMetrics CreateCommandMetrics(ICommandConfigSet configSet, string key, IsolationModeEnum isolationMode)
        {
            if (isolationMode == IsolationModeEnum.SemaphoreIsolation)
            {
                return new SemaphoreIsolationCommandMetrics(configSet, key);
            }
            return new ThreadIsolationCommandMetrics(configSet, key);
        }

        public static ILog CreateLog(Type type)
        {
            return new CLog(type);
        }

        public static ILog CreateLog(ICommandConfigSet configSet, Type type)
        {
            return new CLog(configSet, type);
        }

        public static int? GlobalDefaultCircuitBreakerErrorThresholdPercentage { get; set; }

        public static bool? GlobalDefaultCircuitBreakerForceClosed { get; set; }

        public static int? GlobalDefaultCircuitBreakerRequestCountThreshold { get; set; }

        public static int? GlobalDefaultCommandMaxConcurrentCount { get; set; }

        public static int? GlobalDefaultCommandTimeoutInMilliseconds { get; set; }

        public static int? GlobalDefaultFallbackMaxConcurrentCount { get; set; }
    }
}

