namespace CHystrix.Config
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class ConfigWebServiceConfigItem
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
}

