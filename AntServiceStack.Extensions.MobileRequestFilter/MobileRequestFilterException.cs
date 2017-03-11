using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Extensions.MobileRequestFilter
{
    [Serializable]
    public class MobileRequestFilterException : Exception
    {
        public MobileRequestFilterException()
            : base()
        {
        }

        public MobileRequestFilterException(string message)
            : base(message)
        {
        }

        public MobileRequestFilterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
