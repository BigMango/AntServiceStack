using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Utils
{
    [Serializable]
    public class ServiceInternalException : Exception
    {
        public ServiceInternalException()
        {
        }

        public ServiceInternalException(string message)
            : base(message)
        {
        }

        public ServiceInternalException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
