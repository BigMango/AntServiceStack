namespace CHystrix.Web
{
    using CHystrix.Config;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class CommandConfigInfo
    {
        public override bool Equals(object obj)
        {
            CommandConfigInfo info = obj as CommandConfigInfo;
            if (info == null)
            {
                return false;
            }
            return string.Equals(this.CommandKey, info.CommandKey, StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            if (this.CommandKey != null)
            {
                return this.CommandKey.GetHashCode();
            }
            return 0;
        }

        [DataMember(Order=1)]
        public string CommandKey { get; set; }

        [DataMember(Order=5)]
        public CommandConfigSet ConfigSet { get; set; }

        [DataMember(Order=3)]
        public string Domain { get; set; }

        [DataMember(Order=2)]
        public string GroupKey { get; set; }

        [DataMember(Order=4)]
        public string Type { get; set; }
    }
}

