using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using AntServiceStack.Common.Types;
using AntServiceStack.ServiceHost;

namespace AntServiceStack.ServiceClient
{
    internal static class ExceptionFactory
    {
        public static WebException CreateWebException(WebException webException, string title = null)
        {
            if (webException == null)
                throw new ArgumentNullException("webException");

            switch (webException.Status)
            {
                case WebExceptionStatus.NameResolutionFailure:
                    return new NameResolutionFailureWebException(title ?? webException.Message, webException);

                case WebExceptionStatus.ConnectFailure:
                    return new ConnectFailureWebException(title ?? webException.Message, webException);

                case WebExceptionStatus.ConnectionClosed:
                    return new ConnectionClosedWebException(title ?? webException.Message, webException);

                case WebExceptionStatus.ProtocolError:
                    HttpWebResponse errorResponse = webException.Response as HttpWebResponse;
                    if (errorResponse == null)
                        return new WebProtocolException(title ?? webException.Message, webException);

                    WebProtocolException webProtocolException = CreateProtocolException(webException, title);
                    webProtocolException.StatusCode = (int)errorResponse.StatusCode;
                    webProtocolException.StatusDescription = errorResponse.StatusDescription;
                    return webProtocolException;

                case WebExceptionStatus.Timeout:
                    return new TimeoutWebException(title ?? webException.Message, webException);

                case WebExceptionStatus.RequestCanceled:
                    return new RequestCanceledWebException(title ?? webException.Message, webException);

                default: 
                    return webException;
            }
        }

        private static WebProtocolException CreateProtocolException(WebException webException, string title = null)
        {
            if(webException.Status != WebExceptionStatus.ProtocolError)
                throw new ArgumentException("web exception should be protocol error");

            HttpWebResponse errorResponse = webException.Response as HttpWebResponse;
            if (errorResponse == null)
                throw new ArgumentException("web exception's response should not be null");

            switch ((int)errorResponse.StatusCode)
            {
                case (int)HttpStatusCode.BadRequest:
                    return new BadRequestWebException(title ?? webException.Message, webException);
                case (int)HttpStatusCode.Unauthorized:
                    return new UnauthorizedWebException(title ?? webException.Message, webException);
                case (int)HttpStatusCode.Forbidden:
                    return new ForbiddenWebException(title ?? webException.Message, webException);
                case (int)HttpStatusCode.NotFound:
                    return new NotFoundWebException(title ?? webException.Message, webException);
                case (int)HttpStatusCode.MethodNotAllowed:
                    return new MethodNotAllowedWebException(title ?? webException.Message, webException);
                case 429:
                    return new RateLimitingWebException(title ?? webException.Message, webException);
                case (int)HttpStatusCode.InternalServerError:
                    return new InternalServerErrorWebException(title ?? webException.Message, webException);
                case (int)HttpStatusCode.BadGateway:
                    return new BadGatewayWebException(title ?? webException.Message, webException);
                case (int)HttpStatusCode.ServiceUnavailable:
                    return new ServiceUnavailableWebException(title ?? webException.Message, webException);
                case (int)HttpStatusCode.GatewayTimeout:
                    return new GatewayTimeoutWebException(title ?? webException.Message, webException);
                default:
                    return new WebProtocolException(title ?? webException.Message, webException);
            }
        }

        public static CServiceException CreateCServiceException(List<ErrorDataType> errors)
        {
            if (errors != null && errors.Count > 0 && errors[0] != null)
            {
                var errorData = errors[0];
                switch (errorData.ErrorClassification)
                {
                    case ErrorClassificationCodeType.ServiceError:
                        return new ServiceErrorCServiceException(errorData.Message) { ErrorCode = errorData.ErrorCode };
                    case ErrorClassificationCodeType.ValidationError:
                        return new ValidationErrorCServiceException(errorData.Message) { ErrorCode = errorData.ErrorCode };
                    case ErrorClassificationCodeType.FrameworkError:
                        return new FrameworkErrorCServiceException(errorData.Message) { ErrorCode = errorData.ErrorCode };
                    case ErrorClassificationCodeType.SLAError:
                        return new SLAErrorCServiceException(errorData.Message) { ErrorCode = errorData.ErrorCode };
                    default:
                        return new CServiceException(errorData.Message) { ErrorCode = errorData.ErrorCode };
                }
            }
            else  // should not happen in real case, just for defensive programming
            {
                string message = "Failed response without error data, please file bug to servie owner!";
                return new CServiceException(message) { };
            }
        }
    }
}
