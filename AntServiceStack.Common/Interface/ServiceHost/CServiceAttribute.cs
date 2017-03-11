using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.ServiceHost
{
    /// <summary>
    /// Mark a AntServiceStack supported service
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class AntServiceInterfaceAttribute : Attribute
    {
        internal const string DefaultCodeGeneratorVersion = "1.0.0.0";

        /// <summary>
        /// The version of CTrip Code Generator used to generate this service
        /// </summary>
        public string CodeGeneratorVersion { get; set; }

        /// <summary>
        /// A formally defined service name, extracted from wsdl during code generation
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// A formally defined service namespace, extracted from wsdl during code generation
        /// </summary>
        public string ServiceNamespace { get; set; }

        /// <summary>
        /// Mark a AntServiceStack supported service
        /// </summary>
        public AntServiceInterfaceAttribute()
        {
            CodeGeneratorVersion = DefaultCodeGeneratorVersion;
        }

        /// <summary>
        /// Mark a AntServiceStack supported service
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="serviceNamespace">service name space</param>
        public AntServiceInterfaceAttribute(string serviceName, string serviceNamespace)
            : this(serviceName, serviceNamespace, DefaultCodeGeneratorVersion)
        {
        }

        /// <summary>
        /// Mark a AntServiceStack supported service
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="serviceNamespace">service name space</param>
        /// <param name="version">The version of CTrip Code Generator used to generate this service</param>
        public AntServiceInterfaceAttribute(string serviceName, string serviceNamespace, string version)
        {
            ServiceName = serviceName;
            ServiceNamespace = serviceNamespace;
            CodeGeneratorVersion = version;
        }
    }
}
