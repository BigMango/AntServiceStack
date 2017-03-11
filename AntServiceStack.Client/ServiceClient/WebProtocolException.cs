using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Types;
using AntServiceStack.Text;
using AntServiceStack.ServiceHost;
using System.Net;

namespace AntServiceStack.ServiceClient
{
    /// <summary>
    /// Http协议错误，或者服务调用名错误时抛出的例外。
    /// 
    /// 
    /// This exception is thrown when: 
    /// 1. an error occurs while accessing the network through http(s) protocol,
    /// 2. or client invoked a service with an invalid operation name.
    /// </summary>
    [Serializable]
    public class WebProtocolException
        : WebExceptionBase
    {
        public WebProtocolException() { }
        public WebProtocolException(string message) : base(message) { }
        public WebProtocolException(string message, Exception innerException) : base(message, innerException) { }
        internal WebProtocolException(string message, WebException webException) : base(message, webException) { }

        public int StatusCode { get; set; }

        public string StatusDescription { get; set; }

        public IHasResponseStatus ResponseObject { get; set; }

        public string ResponseBody { get; set; }

        public List<ErrorDataType> ResponseErrors
        {
            get
            {
                if (this.ResponseObject == null || this.ResponseObject.ResponseStatus == null)
                    return null;

                return ResponseObject.ResponseStatus.Errors;
            }
        }
    }
}
