using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Utils;

namespace AntServiceStack.ServiceClient
{
    [Serializable]
    internal class ServiceErrorCServiceException : CServiceException
    {
        internal ServiceErrorCServiceException() { }
        internal ServiceErrorCServiceException(string message) : base(InternalServiceUtils.ServiceErrorTitle + message) { }
    }

    [Serializable]
    internal class ValidationErrorCServiceException : CServiceException
    {
        internal ValidationErrorCServiceException() { }
        internal ValidationErrorCServiceException(string message) : base(InternalServiceUtils.ValidationErrorTitle + message) { }
    }

    [Serializable]
    internal class FrameworkErrorCServiceException : CServiceException
    {
        internal FrameworkErrorCServiceException() { }
        internal FrameworkErrorCServiceException(string message) : base(InternalServiceUtils.FrameworkErrorTitle + message) { }
    }

    [Serializable]
    internal class SLAErrorCServiceException : CServiceException
    {
        internal SLAErrorCServiceException() { }
        internal SLAErrorCServiceException(string message) : base(InternalServiceUtils.SLAErrorTitle + message) 
        {
            this.HelpLink = "http://conf.ctripcorp.com/pages/viewpage.action?pageId=67227657";
        }
    }
}
