namespace CHystrix.Web
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class PercentileInfo
    {
        [DataMember]
        public long P0 { get; set; }

        [DataMember]
        public long P100 { get; set; }

        [DataMember]
        public long P25 { get; set; }

        [DataMember]
        public long P50 { get; set; }

        [DataMember]
        public long P75 { get; set; }

        [DataMember]
        public long P90 { get; set; }

        [DataMember]
        public long P95 { get; set; }

        [DataMember]
        public long P99 { get; set; }

        [DataMember]
        public long P99DOT5 { get; set; }
    }
}

