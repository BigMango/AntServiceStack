namespace CHystrix.Config
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class LookupServiceUrlResponse
    {
        [DataMember]
        public string targetUrl { get; set; }
    }
}

