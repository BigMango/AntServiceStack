using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Utils
{
    [Serializable]
    public class BadRequestException : Exception
    {
        public BadRequestException()
        {
        }

        public BadRequestException(string message)
            : base(message)
        {
        }

        public BadRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
