namespace CHystrix.Config
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [Serializable, DebuggerStepThrough, DataContract(Namespace="http://soa.ant.com/framework/chystrix/configservice/v1"), XmlType(Namespace="http://soa.ant.com/framework/chystrix/configservice/v1"), GeneratedCode("System.Xml", "4.0.30319.1026"), XmlRoot(Namespace="http://soa.ant.com/framework/chystrix/configservice/v1", IsNullable=true), DesignerCategory("code")]
    internal class CHystrixApplication
    {
        private string appNameField;
        private List<CHystrixCommand> commandsField;

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

        [XmlElement("Commands"), DataMember]
        public List<CHystrixCommand> Commands
        {
            get
            {
                if (this.commandsField == null)
                {
                    this.commandsField = new List<CHystrixCommand>();
                }
                return this.commandsField;
            }
            set
            {
                this.commandsField = value;
            }
        }
    }
}

