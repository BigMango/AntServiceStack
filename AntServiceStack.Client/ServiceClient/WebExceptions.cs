using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace AntServiceStack.ServiceClient
{
    [Serializable]
    public class NameResolutionFailureWebException : WebExceptionBase
    {
        internal NameResolutionFailureWebException(string message, WebException webException) : base(message, webException) { }
    }

    [Serializable]
    public class ConnectFailureWebException : WebExceptionBase
    {
        internal ConnectFailureWebException(string message, WebException webException) : base(message, webException) { }
    }

    [Serializable]
    public class ConnectionClosedWebException : WebExceptionBase
    {
        internal ConnectionClosedWebException(string message, WebException webException) : base(message, webException) { }
    }

    [Serializable]
    public class RequestCanceledWebException : WebExceptionBase
    {
        internal RequestCanceledWebException(string message, WebException webException) : base(message, webException) { }
    }

    [Serializable]
    public class TimeoutWebException : WebExceptionBase
    {
        internal TimeoutWebException(string message, WebException webException) : base(message, webException) { }
    }

    [Serializable]
    public class BadRequestWebException : WebProtocolException
    {
        internal BadRequestWebException(string message, WebException webException) : base(message, webException) { }
    }

    [Serializable]
    public class UnauthorizedWebException : WebProtocolException
    {
        internal UnauthorizedWebException(string message, WebException webException) : base(message, webException) { }
    }

    [Serializable]
    public class ForbiddenWebException : WebProtocolException
    {
        internal ForbiddenWebException(string message, WebException webException) : base(message, webException) { }
    }

    [Serializable]
    public class NotFoundWebException : WebProtocolException
    {
        internal NotFoundWebException(string message, WebException webException) : base(message, webException) { }
    }

    [Serializable]
    public class MethodNotAllowedWebException : WebProtocolException
    {
        internal MethodNotAllowedWebException(string message, WebException webException) : base(message, webException) { }
    }

    [Serializable]
    public class RateLimitingWebException : WebProtocolException
    {
        internal RateLimitingWebException(string message, WebException webException) : base(message, webException) { }
    }

    [Serializable]
    public class InternalServerErrorWebException : WebProtocolException
    {
        internal InternalServerErrorWebException(string message, WebException webException) : base(message, webException) { }
    }

    [Serializable]
    public class BadGatewayWebException : WebProtocolException
    {
        internal BadGatewayWebException(string message, WebException webException) : base(message, webException) { }
    }

    [Serializable]
    public class ServiceUnavailableWebException : WebProtocolException
    {
        internal ServiceUnavailableWebException(string message, WebException webException) : base(message, webException) { }
    }

    [Serializable]
    public class GatewayTimeoutWebException : WebProtocolException
    {
        internal GatewayTimeoutWebException(string message, WebException webException) : base(message, webException) { }
    }
}
