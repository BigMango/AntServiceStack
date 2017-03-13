namespace CHystrix.Config
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [Serializable, DesignerCategory("code"), XmlType(Namespace="http://soa.ant.com/framework/chystrix/configservice/v1"), DataContract(Name="GetApplicationConfigRequest", Namespace="http://soa.ant.com/framework/chystrix/configservice/v1"), DebuggerStepThrough, GeneratedCode("System.Xml", "4.0.30319.1026"), XmlRoot("GetApplicationConfigRequest", Namespace="http://soa.ant.com/framework/chystrix/configservice/v1", IsNullable=false)]
    internal class GetApplicationConfigRequestType
    {
        private string appNameField;

        [DataMember]
        public string AppName
        {
            get
            {
                return this.appNameField;
            }
            set
            {
                this.appNameField = value;
            }
        }
    }
}

