using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Utils
{
    [Serializable]
    public class RateLimitingException : Exception
    {
        public RateLimitingException()
        {
        }

        public RateLimitingException(string message)
            : base(message)
        {
        }

        public RateLimitingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
