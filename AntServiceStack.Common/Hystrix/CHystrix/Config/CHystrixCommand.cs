namespace CHystrix.Config
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [Serializable, DebuggerStepThrough, DesignerCategory("code"), GeneratedCode("System.Xml", "4.0.30319.1026"), XmlRoot(Namespace="http://soa.ctrip.com/framework/chystrix/configservice/v1", IsNullable=true), DataContract(Namespace="http://soa.ctrip.com/framework/chystrix/configservice/v1"), XmlType(Namespace="http://soa.ctrip.com/framework/chystrix/configservice/v1")]
    internal class CHystrixCommand
    {
        private CHystrixCommandConfig configField;
        private string keyField;

        [DataMember]
        public CHystrixCommandConfig Config
        {
            get
            {
                return this.configField;
            }
            set
            {
                this.configField = value;
            }
        }

        [DataMember]
        public string Key
        {
            get
            {
                return this.keyField;
            }
            set
            {
                this.keyField = value;
            }
        }
    }
}

