namespace CHystrix.Config
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class LookupServiceUrlRequest
    {
        [DataMember]
        public string ServiceName { get; set; }

        [DataMember]
        public string ServiceNamespace { get; set; }
    }
}

