namespace CHystrix
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class CommandInfo
    {
        [DataMember]
        public string CommandKey { get; set; }

        [DataMember]
        public string Domain { get; set; }

        [DataMember]
        public string GroupKey { get; set; }

        [DataMember]
        public string InstanceKey { get; set; }

        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public string Type { get; set; }
    }
}

