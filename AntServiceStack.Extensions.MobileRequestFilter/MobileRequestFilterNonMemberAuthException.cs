using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Extensions.MobileRequestFilter
{
    [Serializable]
    public class MobileRequestFilterNonMemberAuthException : Exception
    {
        public MobileRequestFilterNonMemberAuthException()
            : base()
        {
        }

        public MobileRequestFilterNonMemberAuthException(string message)
            : base(message)
        {
        }

        public MobileRequestFilterNonMemberAuthException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
