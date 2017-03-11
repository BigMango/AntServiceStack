namespace CHystrix.Web
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class MetricsInfo
    {
        [DataMember(Order=1)]
        public int CommandCount { get; set; }

        [DataMember(Order=3)]
        public List<HystrixCommandInfo> CommandInfoList { get; set; }

        [DataMember(Order=2)]
        public int ThreadPoolCount { get; set; }

        [DataMember(Order=4)]
        public List<HystrixThreadPoolInfo> ThreadPoolInfoList { get; set; }
    }
}

