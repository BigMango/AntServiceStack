using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.ServiceHost;
using AntServiceStack.Common.Types;

namespace AntServiceStack.ServiceClient
{
    /// <summary>
    /// 当CServiceStatckf服务端返回响应居住错误(Response Resident Error)时，该例外会被抛出。
    /// 
    /// This exception is thrown when a AntServiceStack service response resident error(RRE)
    /// is returned to the client.
    /// </summary>
    [Serializable]
    public class CServiceException : Exception
    {
        public CServiceException() { }

        public CServiceException(string message) : base(message) { }

        /// <summary>
        /// Error code of the first error
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// The deserialized response object containing error data
        /// </summary>
        public IHasResponseStatus ResponseObject { get; set; }

        /// <summary>
        /// A list of error data
        /// </summary>
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
