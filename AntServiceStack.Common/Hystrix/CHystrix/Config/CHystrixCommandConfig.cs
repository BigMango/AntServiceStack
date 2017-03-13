namespace CHystrix.Config
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [Serializable, XmlRoot(Namespace="http://soa.ant.com/framework/chystrix/configservice/v1", IsNullable=true), DebuggerStepThrough, XmlType(Namespace="http://soa.ant.com/framework/chystrix/configservice/v1"), GeneratedCode("System.Xml", "4.0.30319.1026"), DataContract(Namespace="http://soa.ant.com/framework/chystrix/configservice/v1"), DesignerCategory("code")]
    internal class CHystrixCommandConfig
    {
        private int? circuitBreakerErrorThresholdPercentageField;
        private bool? circuitBreakerForceClosedField;
        private bool? circuitBreakerForceOpenField;
        private int? circuitBreakerRequestCountThresholdField;
        private int? commandMaxConcurrentCountField;
        private int? commandTimeoutInMillisecondsField;
        private bool? degradeLogLevelField;
        private int? fallbackMaxConcurrentCountField;
        private bool? logExecutionErrorField;
        private int? maxAsyncCommandExceedPercentageField;

        [XmlElement(IsNullable=true), DataMember]
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

        [XmlElement(IsNullable=true), DataMember]
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

        [DataMember, XmlElement(IsNullable=true)]
        public bool? CircuitBreakerForceOpen
        {
            get
            {
                return this.circuitBreakerForceOpenField;
            }
            set
            {
                this.circuitBreakerForceOpenField = value;
            }
        }

        [DataMember, XmlElement(IsNullable=true)]
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

        [XmlElement(IsNullable=true), DataMember]
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

        [XmlElement(IsNullable=true), DataMember]
        public bool? DegradeLogLevel
        {
            get
            {
                return this.degradeLogLevelField;
            }
            set
            {
                this.degradeLogLevelField = value;
            }
        }

        [XmlElement(IsNullable=true), DataMember]
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

        [DataMember, XmlElement(IsNullable=true)]
        public bool? LogExecutionError
        {
            get
            {
                return this.logExecutionErrorField;
            }
            set
            {
                this.logExecutionErrorField = value;
            }
        }

        [DataMember, XmlElement(IsNullable=true)]
        public int? MaxAsyncCommandExceedPercentage
        {
            get
            {
                return this.maxAsyncCommandExceedPercentageField;
            }
            set
            {
                this.maxAsyncCommandExceedPercentageField = value;
            }
        }
    }
}

