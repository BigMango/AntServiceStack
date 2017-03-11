namespace CHystrix.Web
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class HystrixThreadPoolInfo
    {
        [DataMember]
        public int currentActiveCount { get; set; }

        [DataMember]
        public int currentCompletedTaskCount { get; set; }

        [DataMember]
        public int currentCorePoolSize { get; set; }

        [DataMember]
        public int currentLargestPoolSize { get; set; }

        [DataMember]
        public int currentMaximumPoolSize { get; set; }

        [DataMember]
        public int currentPoolSize { get; set; }

        [DataMember]
        public int currentQueueSize { get; set; }

        [DataMember]
        public int currentTaskCount { get; set; }

        [DataMember]
        public long currentTime { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public int propertyValue_metricsRollingStatisticalWindowInMilliseconds { get; set; }

        [DataMember]
        public int propertyValue_queueSizeRejectionThreshold { get; set; }

        [DataMember]
        public int reportingHosts { get; set; }

        [DataMember]
        public int rollingCountThreadsExecuted { get; set; }

        [DataMember]
        public int rollingMaxActiveThreads { get; set; }

        [DataMember]
        public string type { get; set; }
    }
}

