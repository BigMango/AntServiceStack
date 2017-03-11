namespace CHystrix.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class RegisterCommandInfo
    {
        [DataMember]
        public string Domain { get; set; }

        [DataMember]
        public string GroupKey { get; set; }

        [DataMember]
        public List<string> InstanceKeys { get; set; }

        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public string Type { get; set; }
    }
}

