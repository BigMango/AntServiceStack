namespace CHystrix.Config
{
    using CHystrix;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Threading;

    [DataContract]
    internal class CommandConfigSet : ICommandConfigSet, IConfigChangeEvent
    {
        private int? _circuitBreakerErrorThresholdPercentage;
        private bool? _circuitBreakerForceClosed;
        private int? _circuitBreakerRequestCountThreshold;
        private int _circuitBreakerSleepWindowInMilliseconds;
        private int? _commandMaxConcurrentCount;
        private int? _commandTimeoutInMilliseconds;
        private int? _fallbackMaxConcurrentCount;
        private int _maxAsyncCommandExceedPercentage;
        private int _metricsHealthSnapshotIntervalInMilliseconds;
        private int _metricsRollingPercentileBucketSize;
        private int _metricsRollingPercentileWindowBuckets;
        private int _metricsRollingPercentileWindowInMilliseconds;
        private int _metricsRollingStatisticalWindowBuckets;
        private int _metricsRollingStatisticalWindowInMilliseconds;
        private readonly object EventLock = new object();

        event HandleConfigChangeDelegate IConfigChangeEvent.OnConfigChanged
        {
            add
            {
                lock (this.EventLock)
                {
                    this.onConfigChanged += value;
                }
            }
            remove
            {
                lock (this.EventLock)
                {
                    this.onConfigChanged -= value;
                }
            }
        }

        private event HandleConfigChangeDelegate onConfigChanged;

        void IConfigChangeEvent.RaiseConfigChangeEvent()
        {
            if (this.onConfigChanged != null)
            {
                this.onConfigChanged(this);
            }
        }

        [DataMember]
        public bool CircuitBreakerEnabled { get; set; }

        [DataMember]
        public int CircuitBreakerErrorThresholdPercentage
        {
            get
            {
                if (this._circuitBreakerErrorThresholdPercentage.HasValue)
                {
                    return this._circuitBreakerErrorThresholdPercentage.Value;
                }
                if (ComponentFactory.GlobalDefaultCircuitBreakerErrorThresholdPercentage.HasValue)
                {
                    return ComponentFactory.GlobalDefaultCircuitBreakerErrorThresholdPercentage.Value;
                }
                return 50;
            }
            set
            {
                if ((value > 0) && (value <= 100))
                {
                    this._circuitBreakerErrorThresholdPercentage = new int?(value);
                }
            }
        }

        [DataMember]
        public bool CircuitBreakerForceClosed
        {
            get
            {
                if (this._circuitBreakerForceClosed.HasValue)
                {
                    return this._circuitBreakerForceClosed.Value;
                }
                if (ComponentFactory.GlobalDefaultCircuitBreakerForceClosed.HasValue)
                {
                    return ComponentFactory.GlobalDefaultCircuitBreakerForceClosed.Value;
                }
                return false;
            }
            set
            {
                this._circuitBreakerForceClosed = new bool?(value);
            }
        }

        [DataMember]
        public bool CircuitBreakerForceOpen { get; set; }

        [DataMember]
        public int CircuitBreakerRequestCountThreshold
        {
            get
            {
                if (this._circuitBreakerRequestCountThreshold.HasValue)
                {
                    return this._circuitBreakerRequestCountThreshold.Value;
                }
                if (ComponentFactory.GlobalDefaultCircuitBreakerRequestCountThreshold.HasValue)
                {
                    return ComponentFactory.GlobalDefaultCircuitBreakerRequestCountThreshold.Value;
                }
                return 20;
            }
            set
            {
                if (value >= 0)
                {
                    this._circuitBreakerRequestCountThreshold = new int?(value);
                }
            }
        }

        [DataMember]
        public int CircuitBreakerSleepWindowInMilliseconds
        {
            get
            {
                return this._circuitBreakerSleepWindowInMilliseconds;
            }
            set
            {
                if (value > 0)
                {
                    this._circuitBreakerSleepWindowInMilliseconds = value;
                }
            }
        }

        [DataMember]
        public int CommandMaxConcurrentCount
        {
            get
            {
                if (this._commandMaxConcurrentCount.HasValue)
                {
                    return this._commandMaxConcurrentCount.Value;
                }
                if (this.IsolationMode == IsolationModeEnum.ThreadIsolation)
                {
                    return 20;
                }
                if (ComponentFactory.GlobalDefaultCommandMaxConcurrentCount.HasValue)
                {
                    return ComponentFactory.GlobalDefaultCommandMaxConcurrentCount.Value;
                }
                return 100;
            }
            set
            {
                if (value > 0)
                {
                    this._commandMaxConcurrentCount = new int?(value);
                }
            }
        }

        [DataMember]
        public int CommandTimeoutInMilliseconds
        {
            get
            {
                if (this._commandTimeoutInMilliseconds.HasValue)
                {
                    return this._commandTimeoutInMilliseconds.Value;
                }
                if (ComponentFactory.GlobalDefaultCommandTimeoutInMilliseconds.HasValue)
                {
                    return ComponentFactory.GlobalDefaultCommandTimeoutInMilliseconds.Value;
                }
                return 0x7530;
            }
            set
            {
                if (value > 0)
                {
                    this._commandTimeoutInMilliseconds = new int?(value);
                }
            }
        }

        [DataMember]
        public bool DegradeLogLevel { get; set; }

        [DataMember]
        public int FallbackMaxConcurrentCount
        {
            get
            {
                if (this._fallbackMaxConcurrentCount.HasValue)
                {
                    return this._fallbackMaxConcurrentCount.Value;
                }
                if (this.IsolationMode == IsolationModeEnum.ThreadIsolation)
                {
                    return 20;
                }
                if (ComponentFactory.GlobalDefaultFallbackMaxConcurrentCount.HasValue)
                {
                    return ComponentFactory.GlobalDefaultFallbackMaxConcurrentCount.Value;
                }
                return 100;
            }
            set
            {
                if (value > 0)
                {
                    this._fallbackMaxConcurrentCount = new int?(value);
                }
            }
        }

        public IsolationModeEnum IsolationMode { get; set; }

        [DataMember]
        public bool LogExecutionError { get; set; }

        [DataMember]
        public int MaxAsyncCommandExceedPercentage
        {
            get
            {
                return this._maxAsyncCommandExceedPercentage;
            }
            set
            {
                if ((value >= 0) && (value <= 100))
                {
                    this._maxAsyncCommandExceedPercentage = value;
                }
            }
        }

        [DataMember]
        public int MetricsHealthSnapshotIntervalInMilliseconds
        {
            get
            {
                return this._metricsHealthSnapshotIntervalInMilliseconds;
            }
            set
            {
                if (value > 0)
                {
                    this._metricsHealthSnapshotIntervalInMilliseconds = value;
                }
            }
        }

        [DataMember]
        public int MetricsRollingPercentileBucketSize
        {
            get
            {
                return this._metricsRollingPercentileBucketSize;
            }
            set
            {
                if (value > 0)
                {
                    this._metricsRollingPercentileBucketSize = value;
                }
            }
        }

        [DataMember]
        public bool MetricsRollingPercentileEnabled { get; set; }

        [DataMember]
        public int MetricsRollingPercentileWindowBuckets
        {
            get
            {
                return this._metricsRollingPercentileWindowBuckets;
            }
            set
            {
                if (value > 0)
                {
                    this._metricsRollingPercentileWindowBuckets = value;
                }
            }
        }

        [DataMember]
        public int MetricsRollingPercentileWindowInMilliseconds
        {
            get
            {
                return this._metricsRollingPercentileWindowInMilliseconds;
            }
            set
            {
                if (value > 0)
                {
                    this._metricsRollingPercentileWindowInMilliseconds = value;
                }
            }
        }

        [DataMember]
        public int MetricsRollingStatisticalWindowBuckets
        {
            get
            {
                return this._metricsRollingStatisticalWindowBuckets;
            }
            set
            {
                if (value > 0)
                {
                    this._metricsRollingStatisticalWindowBuckets = value;
                }
            }
        }

        [DataMember]
        public int MetricsRollingStatisticalWindowInMilliseconds
        {
            get
            {
                return this._metricsRollingStatisticalWindowInMilliseconds;
            }
            set
            {
                if (value > 0)
                {
                    this._metricsRollingStatisticalWindowInMilliseconds = value;
                }
            }
        }
    }
}

