using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Ant.Tools.SOA.CodeGeneration.Exceptions
{
    /// <summary>
    /// Exception indicating a violation of  SOA Policy in service wsdl.
    /// </summary>
    public class SOAPolicyViolationException : Exception
    {
        #region Constructors

        public SOAPolicyViolationException()
        {
        }

        public SOAPolicyViolationException(string message)
            : base(message)
        {
        }

        public SOAPolicyViolationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SOAPolicyViolationException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        #endregion
    }
}
