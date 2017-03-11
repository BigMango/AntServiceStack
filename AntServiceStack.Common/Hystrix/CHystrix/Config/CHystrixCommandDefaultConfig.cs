namespace CHystrix.Config
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [Serializable, XmlType(Namespace="http://soa.ctrip.com/framework/chystrix/configservice/v1"), XmlRoot(Namespace="http://soa.ctrip.com/framework/chystrix/configservice/v1", IsNullable=true), DataContract(Namespace="http://soa.ctrip.com/framework/chystrix/configservice/v1"), DesignerCategory("code"), GeneratedCode("System.Xml", "4.0.30319.1026"), DebuggerStepThrough]
    internal class CHystrixCommandDefaultConfig
    {
        private int? circuitBreakerErrorThresholdPercentageField;
        private bool? circuitBreakerForceClosedField;
        private int? circuitBreakerRequestCountThresholdField;
        private int? commandMaxConcurrentCountField;
        private int? commandTimeoutInMillisecondsField;
        private int? fallbackMaxConcurrentCountField;

        [DataMember, XmlElement(IsNullable=true)]
        public int? CircuitBreakerErrorThresholdPercentage
        {
            get
            {
                return this.circuitBreakerErrorThresholdPercentageField;
            }
            set
            {
                this.circuitBreakerErrorThresholdPercentageField = value;
            }
        }

        [DataMember, XmlElement(IsNullable=true)]
        public bool? CircuitBreakerForceClosed
        {
            get
            {
                return this.circuitBreakerForceClosedField;
            }
            set
            {
                this.circuitBreakerForceClosedField = value;
            }
        }

        [XmlElement(IsNullable=true), DataMember]
        public int? CircuitBreakerRequestCountThreshold
        {
            get
            {
                return this.circuitBreakerRequestCountThresholdField;
            }
            set
            {
                this.circuitBreakerRequestCountThresholdField = value;
            }
        }

        [XmlElement(IsNullable=true), DataMember]
        public int? CommandMaxConcurrentCount
        {
            get
            {
                return this.commandMaxConcurrentCountField;
            }
            set
            {
                this.commandMaxConcurrentCountField = value;
            }
        }

        [DataMember, XmlElement(IsNullable=true)]
        public int? CommandTimeoutInMilliseconds
        {
            get
            {
                return this.commandTimeoutInMillisecondsField;
            }
            set
            {
                this.commandTimeoutInMillisecondsField = value;
            }
        }

        [DataMember, XmlElement(IsNullable=true)]
        public int? FallbackMaxConcurrentCount
        {
            get
            {
                return this.fallbackMaxConcurrentCountField;
            }
            set
            {
                this.fallbackMaxConcurrentCountField = value;
            }
        }
    }
}

