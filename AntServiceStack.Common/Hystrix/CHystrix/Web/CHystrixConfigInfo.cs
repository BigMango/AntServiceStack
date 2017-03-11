namespace CHystrix.Web
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class CHystrixConfigInfo
    {
        [DataMember(Order=1)]
        public string ApplicationPath { get; set; }

        [DataMember(Order=2)]
        public string CHystrixAppName { get; set; }

        [DataMember(Order=8)]
        public string CHystrixConfigServiceUrl { get; set; }

        [DataMember(Order=9)]
        public string CHystrixRegistryServiceUrl { get; set; }

        [DataMember(Order=3)]
        public string CHystrixVersion { get; set; }

        [DataMember(Order=0x24)]
        public List<CommandConfigInfo> CommandConfigInfoList { get; set; }

        [DataMember(Order=5)]
        public int CommandCount { get; set; }

        [DataMember(Order=6)]
        public string ConfigWebServiceUrl { get; set; }

        [DataMember(Order=0x1f)]
        public int? DefaultCircuitBreakerErrorThresholdPercentage { get; set; }

        [DataMember(Order=0x20)]
        public bool? DefaultCircuitBreakerForceClosed { get; set; }

        [DataMember(Order=30)]
        public int? DefaultCircuitBreakerRequestCountThreshold { get; set; }

        [DataMember(Order=0x21)]
        public int? DefaultCommandTimeoutInMilliseconds { get; set; }

        [DataMember(Order=0x22)]
        public int? DefaultSemaphoreIsolationMaxConcurrentCount { get; set; }

        [DataMember(Order=0x23)]
        public int? DefaultThreadIsolationMaxConcurrentCount { get; set; }

        [DataMember(Order=14)]
        public int FrameworkDefaultCircuitBreakerErrorThresholdPercentage { get; set; }

        [DataMember(Order=15)]
        public bool FrameworkDefaultCircuitBreakerForceClosed { get; set; }

        [DataMember(Order=13)]
        public int FrameworkDefaultCircuitBreakerRequestCountThreshold { get; set; }

        [DataMember(Order=0x10)]
        public int FrameworkDefaultCommandTimeoutInMilliseconds { get; set; }

        [DataMember(Order=0x11)]
        public int FrameworkDefaultSemaphoreIsolationMaxConcurrentCount { get; set; }

        [DataMember(Order=0x12)]
        public int FrameworkDefaultThreadIsolationMaxConcurrentCount { get; set; }

        [DataMember(Order=0x19)]
        public int? GlobalDefaultCircuitBreakerErrorThresholdPercentage { get; set; }

        [DataMember(Order=0x1a)]
        public bool? GlobalDefaultCircuitBreakerForceClosed { get; set; }

        [DataMember(Order=0x18)]
        public int? GlobalDefaultCircuitBreakerRequestCountThreshold { get; set; }

        [DataMember(Order=0x1c)]
        public int? GlobalDefaultCommandMaxConcurrentCount { get; set; }

        [DataMember(Order=0x1b)]
        public int? GlobalDefaultCommandTimeoutInMilliseconds { get; set; }

        [DataMember(Order=0x1d)]
        public int? GlobalDefaultFallbackMaxConcurrentCount { get; set; }

        [DataMember(Order=4)]
        public int MaxCommandCount { get; set; }

        [DataMember(Order=20)]
        public int MinGlobalDefaultCircuitBreakerErrorThresholdPercentage { get; set; }

        [DataMember(Order=0x13)]
        public int MinGlobalDefaultCircuitBreakerRequestCountThreshold { get; set; }

        [DataMember(Order=0x15)]
        public int MinGlobalDefaultCommandMaxConcurrentCount { get; set; }

        [DataMember(Order=0x17)]
        public int MinGlobalDefaultCommandTimeoutInMilliseconds { get; set; }

        [DataMember(Order=0x16)]
        public int MinGlobalDefaultFallbackMaxConcurrentCount { get; set; }

        [DataMember(Order=12)]
        public int SelfRegistrationIntervalMilliseconds { get; set; }

        [DataMember(Order=7)]
        public string SOARegistryServiceUrl { get; set; }

        [DataMember(Order=10)]
        public int SyncCHystrixConfigIntervalMilliseconds { get; set; }

        [DataMember(Order=11)]
        public int SyncCommandConfigIntervalMilliseconds { get; set; }
    }
}

