using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace AntServiceStack.ServiceClient
{
    [Serializable]
    public class WebExceptionBase : WebException
    {
        internal WebExceptionBase() { }
        internal WebExceptionBase(string message) : base(message) { }
        internal WebExceptionBase(string message, Exception innerException) : base(message, innerException) { }
        internal WebExceptionBase(string message, WebException webException)
            : base(message, webException, webException.Status, webException.Response)
        { }
    }
}
