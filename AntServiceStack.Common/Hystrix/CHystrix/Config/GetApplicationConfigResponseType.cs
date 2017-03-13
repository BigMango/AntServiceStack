namespace CHystrix.Config
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [Serializable, DebuggerStepThrough, DesignerCategory("code"), XmlType(Namespace="http://soa.ant.com/framework/chystrix/configservice/v1"), DataContract(Name="GetApplicationConfigResponse", Namespace="http://soa.ant.com/framework/chystrix/configservice/v1"), XmlRoot("GetApplicationConfigResponse", Namespace="http://soa.ant.com/framework/chystrix/configservice/v1", IsNullable=false), GeneratedCode("System.Xml", "4.0.30319.1026")]
    internal class GetApplicationConfigResponseType
    {
        private CHystrixApplication applicationField;
        private CHystrixCommandDefaultConfig defaultConfigField;

        [DataMember]
        public CHystrixApplication Application
        {
            get
            {
                return this.applicationField;
            }
            set
            {
                this.applicationField = value;
            }
        }

        [DataMember]
        public CHystrixCommandDefaultConfig DefaultConfig
        {
            get
            {
                return this.defaultConfigField;
            }
            set
            {
                this.defaultConfigField = value;
            }
        }
    }
}

