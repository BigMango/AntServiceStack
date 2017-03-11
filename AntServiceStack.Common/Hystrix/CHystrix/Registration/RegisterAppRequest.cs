namespace CHystrix.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class RegisterAppRequest
    {
        [DataMember]
        public string ApplicationPath { get; set; }

        [DataMember]
        public string AppName { get; set; }

        [DataMember]
        public string HostIP { get; set; }

        [DataMember]
        public List<RegisterCommandInfo> HystrixCommands { get; set; }

        [DataMember]
        public string HystrixVersion { get; set; }
    }
}

