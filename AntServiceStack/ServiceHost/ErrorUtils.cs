using System;
using System.Collections.Generic;
using System.Configuration;
using AntServiceStack.Common;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Web;
using AntServiceStack.Common.Types;

using AntServiceStack.Text;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Support;
using AntServiceStack.Validation;
using AntServiceStack.Common.Hystrix;
using Freeway.Logging;

namespace AntServiceStack.ServiceHost
{    
    public static class ErrorUtils
    {
        private static ILog Log = LogManager.GetLogger(typeof(ErrorUtils));

        internal static Dictionary<Type, Tuple<string, LogLevel>> ServiceExceptionErrorCodeMap = new Dictionary<Type, Tuple<string, LogLevel>>();

        /// <summary>
        /// Handle validation errors
        /// </summary>
        /// <param name="validationErrorResult"></param>
        /// <param name="responseType"></param>
        /// <returns></returns>
        public static object CreateValidationErrorResponse(IHttpRequest httpReq, ValidationError validationError, Type responseType)
        {
            var error = CreateErrorData(validationError.ErrorCode, validationError.ErrorMessage, validationError.Violations);
            error.SeverityCode = SeverityCodeType.Error;
            error.ErrorClassification = ErrorClassificationCodeType.ValidationError;
            var errors = new List<ErrorDataType>() { error };
            var errorResponse = CreateErrorResponseDto(
                errors,
                responseType);

            LogError("Validation Exception", httpReq, validationError, false, "FXD300004");

            return errorResponse;
        }

        /// <summary>
        /// Create an instance of the service response dto type and inject it with the supplied error data
        /// </summary>
        /// <param name="responseType"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private static object CreateErrorResponseDto(List<ErrorDataType> errors, Type responseType)
        {
            // Predict the Response message type name
            var responseDtoType = responseType;
            if (responseDtoType == null)
            {
                responseDtoType = typeof(GenericErrorResponseType);
            }

            var responseDto = responseDtoType.CreateInstance();
            if (responseDto == null)
                return null;

            var commonResponseDto = responseDto as IHasResponseStatus;
            if (commonResponseDto != null)
            {
                // defensive programming
                if (commonResponseDto.ResponseStatus == null)
                {
                    commonResponseDto.ResponseStatus = new ResponseStatusType();
                }
                commonResponseDto.ResponseStatus.Ack = AckCodeType.Failure;
                commonResponseDto.ResponseStatus.Timestamp = DateTime.Now;
                commonResponseDto.ResponseStatus.Errors = errors;
            }
            else
            {
                // Just to prevent hacking
                responseDtoType = typeof(GenericErrorResponseType);
                responseDto = responseDtoType.CreateInstance();
                commonResponseDto = responseDto as IHasResponseStatus;
                // defensive programming
                if (commonResponseDto.ResponseStatus == null)
                {
                    commonResponseDto.ResponseStatus = new ResponseStatusType();
                }
                commonResponseDto.ResponseStatus.Ack = AckCodeType.Failure;
                commonResponseDto.ResponseStatus.Timestamp = DateTime.Now;
                commonResponseDto.ResponseStatus.Errors = errors;
            }

            // Return an Error DTO with the exception populated
            return responseDto;
        }

        /// <summary>
        /// Create SLA Error response
        /// </summary>
        /// <param name="hystrixEvent">the hystrix event caused the error</param>
        /// <param name="responseType">target response type</param>
        /// <returns></returns>
        public static object CreateSLAErrorResponse(HystrixEventType hystrixEvent, Type responseType)
        {
            var error = new ErrorDataType();
            if (hystrixEvent == HystrixEventType.ShortCircuited)
            {
                error.Message = "Server entered into self-protecting mode";
                error.ErrorCode = hystrixEvent.ToString();
            }
            if (hystrixEvent == HystrixEventType.ThreadPoolRejected)
            {
                error.Message = "Server entered into rate-limiting mode";
                error.ErrorCode = hystrixEvent.ToString();
            }
            error.SeverityCode = SeverityCodeType.Error;
            error.ErrorClassification = ErrorClassificationCodeType.SLAError;

            var errors = new List<ErrorDataType>() { error };

            var errorResponse = CreateErrorResponseDto(errors, responseType);

            if (HostContext.Instance.Request != null)
            {
                var metadata = EndpointHost.MetadataMap[HostContext.Instance.Request.ServicePath];
                var additionalData = new Dictionary<string, string>();
                additionalData.Add("Service", metadata.FullServiceName);
                additionalData.Add("Operation", HostContext.Instance.Request.OperationName);
                additionalData.Add("ErrorCode", "FXD300005");
                Log.Error("SLA error occurred: Circuit Breaker is open.", additionalData);
            }

            return errorResponse;
        }

        /// <summary>
        /// Handle error thrown by soa framework
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="responseType"></param>
        /// <returns></returns>
        public static object CreateFrameworkErrorResponse(IHttpRequest httpReq, Exception ex, Type responseType, bool isCritical = true, string errorCode = "FXD300000")
        {
            var error = ex.ToErrorData();
            error.SeverityCode = SeverityCodeType.Error;
            error.ErrorClassification = ErrorClassificationCodeType.FrameworkError;
            var errors = new List<ErrorDataType>() { error };

            if (EndpointHost.DebugMode)
            {
                error.StackTrace = ex.StackTrace;
            }

            if (HostContext.Instance.Request != null && HostContext.Instance.Request.RequestObject == null)
            {
                isCritical = false;
                errorCode = "FXD300001";
            }
            LogError("SOA Framework Exception", httpReq, ex, isCritical, errorCode ?? "FXD300000");

            error.StackTrace = string.Empty; // clear stack trace after log exception
            var errorResponse = CreateErrorResponseDto(errors, responseType);

            return errorResponse;
        }

        /// <summary>
        /// Handle exception thrown by service implementation
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ex"></param>
        /// <param name="responseType"></param>
        /// <returns></returns>
        public static object CreateServiceErrorResponse(IHttpRequest httpReq, Exception ex, Type responseType, bool isCritical = true)
        {
            var validationError = ex as ValidationError;
            // in case ValidationError is thrown in service implementation,
            // treate it as ValidationError instead of ServiceError classification
            if (validationError != null)
            {
                return CreateValidationErrorResponse(httpReq, validationError, responseType);
            }

            if (EndpointHost.Config != null && EndpointHost.Config.ReturnsInnerException
                && ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            var error = ex.ToErrorData();
            error.SeverityCode = SeverityCodeType.Error;
            error.ErrorClassification = ErrorClassificationCodeType.ServiceError;
            var errors = new List<ErrorDataType>() { error };

            if (EndpointHost.DebugMode)
            {
                // View stack trace in tests and on the client
                error.StackTrace = string.Empty;
                if (httpReq.RequestObject != null)
                    error.StackTrace = GetRequestErrorBody(httpReq.RequestObject) + "\n";
                error.StackTrace += ex;
            }

            string errorCode = "FXD300002";
            LogLevel logLevel = isCritical ? LogLevel.ERROR : LogLevel.WARN;
            Type exceptionType = ex == null ? null : ex.GetType();
            if (exceptionType != null && ServiceExceptionErrorCodeMap.ContainsKey(exceptionType))
            {
                errorCode = ServiceExceptionErrorCodeMap[exceptionType].Item1;
                logLevel = ServiceExceptionErrorCodeMap[exceptionType].Item2;
            }
            LogError("Service Exception", httpReq, ex, errorCode, logLevel);

            error.StackTrace = string.Empty; // clear stack trace after log exception
            var errorResponse = CreateErrorResponseDto(errors, responseType);

            return errorResponse;
        }

        public static ErrorDataType ToErrorData(this Exception exception)
        {
            var errorDataConverter = exception as IErrorDataConvertable;
            if (errorDataConverter != null)
            {
                return errorDataConverter.ToErrorData();
            }

            return CreateErrorData(exception.GetType().Name, exception.Message, null);
        }

        /// <summary>
        /// Override to provide additional/less context about the Service Exception. 
        /// By default the request is serialized and appended to the ResponseStatus StackTrace.
        /// </summary>
        public static string GetRequestErrorBody(object request)
        {
            var requestString = "";
            try
            {
                requestString = TypeSerializer.SerializeToString(request);
            }
            catch /*(Exception ignoreSerializationException)*/
            {
                //Serializing request successfully is not critical and only provides added error info
            }

            return String.Format("[{0}: {1}]:\n[REQUEST: {2}]", (request ?? new object()).GetType().Name, DateTime.UtcNow, requestString);
        }


        /// <summary>
        /// Creates the error data from the values provided.
        /// 
        /// If the errorCode is empty it will use the first validation error code, 
        /// if there is none it will throw an error.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="validationErrors">The validation errors.</param>
        /// <returns></returns>
        public static ErrorDataType CreateErrorData(string errorCode, string errorMessage, IEnumerable<ValidationErrorField> validationErrors)
        {
            var to = new ErrorDataType
            {
                ErrorCode = errorCode,
                Message = errorMessage,
                ErrorFields = new List<ErrorFieldType>(),
            };
            if (validationErrors != null)
            {
                foreach (var validationError in validationErrors)
                {
                    var error = new ErrorFieldType
                    {
                        ErrorCode = validationError.ErrorCode,
                        FieldName = validationError.FieldName,
                        Message = validationError.ErrorMessage,
                    };
                    to.ErrorFields.Add(error);

                    if (string.IsNullOrEmpty(to.ErrorCode))
                    {
                        to.ErrorCode = validationError.ErrorCode;
                    }
                    if (string.IsNullOrEmpty(to.Message))
                    {
                        to.Message = validationError.ErrorMessage;
                    }
                }
            }
            if (string.IsNullOrEmpty(errorCode))
            {
                if (string.IsNullOrEmpty(to.ErrorCode))
                {
                    throw new ArgumentException("Cannot create a valid error response with an empty errorCode and an empty validationError list");
                }
            }
            return to;
        }

        public static void LogError(string title, IHttpRequest httpRequest)
        {
            LogError(title, httpRequest, default(Exception));
        }

        public static void LogError(string title, IHttpRequest httpRequest, Exception ex)
        {
            LogError(title, httpRequest, ex, true);
        }

        public static void LogError(string title, IHttpRequest httpRequest, Exception ex, bool isCritical)
        {
            LogError(title, httpRequest, ex, isCritical, null);
        }

        public static void LogError(string title, IHttpRequest httpRequest, Exception ex, bool isCritical, string errorCode)
        {
            LogError(title, httpRequest, ex, errorCode, isCritical ? LogLevel.ERROR : LogLevel.WARN);
        }

        internal static void LogError(string title, IHttpRequest httpRequest, Exception ex, string errorCode, LogLevel logLevel)
        {
            if (httpRequest == null)
                return;

            ServiceMetadata metadata = EndpointHost.Config.MetadataMap[httpRequest.ServicePath];
            Dictionary<string, string> additionalExceptionInfo = new Dictionary<string, string>()
            {
                { "Operation", metadata.FullServiceName + "." + httpRequest.OperationName }
            };

            string clientAppId = httpRequest.GetClientAppId();
            if (!string.IsNullOrWhiteSpace(clientAppId))
                additionalExceptionInfo.Add("ClientAppId", clientAppId);

            if (!string.IsNullOrWhiteSpace(errorCode))
                additionalExceptionInfo.Add("ErrorCode", errorCode);

            if (metadata.LogErrorWithRequestInfo)
            {
                try
                {
                    try
                    {
                        var requestInfo = RequestInfoHandler.GetRequestInfo(httpRequest);
                        additionalExceptionInfo["RequestInfo"] = TypeSerializer.SerializeToString(requestInfo);
                    }
                    catch
                    {
                        Dictionary<string, string> requestInfo = new Dictionary<string, string>()
                        {
                            { "Url", httpRequest.RawUrl },
                            { "PathInfo", httpRequest.PathInfo },
                            { "QueryString", TypeSerializer.SerializeToString(httpRequest.QueryString) },
                            { "Headers", TypeSerializer.SerializeToString(httpRequest.Headers) },
                            { "FormData", TypeSerializer.SerializeToString(httpRequest.FormData) }
                        };
                        additionalExceptionInfo["RequestInfo"] = TypeSerializer.SerializeToString(requestInfo);
                    }
                }
                catch
                {
                }

                if (httpRequest.RequestObject != null && !(httpRequest.RequestObject is RequestInfoResponse))
                    additionalExceptionInfo["RequestObject"] = TypeSerializer.SerializeToString(httpRequest.RequestObject);
            }

            switch(logLevel)
            {
                case LogLevel.DEBUG:
                    if (ex != null)
                        Log.Error(title, ex, additionalExceptionInfo);
                    else
                        Log.Error(title, string.Empty, additionalExceptionInfo);
                    break;
                case LogLevel.INFO:
                    if (ex != null)
                        Log.Info(title, ex, additionalExceptionInfo);
                    else
                        Log.Info(title, string.Empty, additionalExceptionInfo);
                    break;
                case LogLevel.WARN:
                    if (ex != null)
                        Log.Warn(title, ex, additionalExceptionInfo);
                    else
                        Log.Warn(title, string.Empty, additionalExceptionInfo);
                    break;
                case LogLevel.ERROR:
                    if (ex != null)
                        Log.Error(title, ex, additionalExceptionInfo);
                    else
                        Log.Error(title, string.Empty, additionalExceptionInfo);
                    break;
                case LogLevel.FATAL:
                    if (ex != null)
                        Log.Fatal(title, ex, additionalExceptionInfo);
                    else
                        Log.Fatal(title, string.Empty, additionalExceptionInfo);
                    break;
            }
        }

        public static void LogError(string title, IHttpRequest httpRequest, ResponseStatusType responseStatus)
        {
            LogError(title, httpRequest, responseStatus, true);
        }

        public static void LogError(string title, IHttpRequest httpRequest, ResponseStatusType responseStatus, bool isCritical)
        {
            LogError(title, httpRequest, responseStatus, isCritical, null);
        }

        public static void LogError(string title, IHttpRequest httpRequest, ResponseStatusType responseStatus, bool isCritical, string errorCode)
        {
            LogError(title, httpRequest, responseStatus, errorCode, isCritical ? LogLevel.ERROR : LogLevel.WARN);
        }

        internal static void LogError(string title, IHttpRequest httpRequest, ResponseStatusType responseStatus, string errorCode, LogLevel logLevel)
        {
            if (httpRequest == null)
                return;

            ServiceMetadata metadata = EndpointHost.Config.MetadataMap[httpRequest.ServicePath];
            Dictionary<string, string> additionalExceptionInfo = new Dictionary<string, string>()
            {
                { "Version", ServiceUtils.SOA2VersionCatName },
                { "Operation", metadata.FullServiceName + "." + httpRequest.OperationName }
            };

            string clientAppId = httpRequest.GetClientAppId();
            if (!string.IsNullOrWhiteSpace(clientAppId))
                additionalExceptionInfo.Add("ClientAppId", clientAppId);

            if (!string.IsNullOrWhiteSpace(errorCode))
                additionalExceptionInfo.Add("ErrorCode", errorCode);

            if (metadata.LogErrorWithRequestInfo)
            {
                try
                {
                    try
                    {
                        var requestInfo = RequestInfoHandler.GetRequestInfo(httpRequest);
                        additionalExceptionInfo["RequestInfo"] = TypeSerializer.SerializeToString(requestInfo);
                    }
                    catch
                    {
                        Dictionary<string, string> requestInfo = new Dictionary<string, string>()
                        {
                            { "Url", httpRequest.RawUrl },
                            { "PathInfo", httpRequest.PathInfo },
                            { "QueryString", TypeSerializer.SerializeToString(httpRequest.QueryString) },
                            { "Headers", TypeSerializer.SerializeToString(httpRequest.Headers) },
                            { "FormData", TypeSerializer.SerializeToString(httpRequest.FormData) }
                        };
                        additionalExceptionInfo["RequestInfo"] = TypeSerializer.SerializeToString(requestInfo);
                    }
                }
                catch
                {
                }

                if (httpRequest.RequestObject != null && !(httpRequest.RequestObject is RequestInfoResponse))
                    additionalExceptionInfo["RequestObject"] = TypeSerializer.SerializeToString(httpRequest.RequestObject);
            }

            string message = string.Empty;
            if (responseStatus != null)
            {
                additionalExceptionInfo["ResponseStatus"] = TypeSerializer.SerializeToString(responseStatus);
                if (responseStatus.Errors != null && responseStatus.Errors.Count > 0 && responseStatus.Errors[0] != null)
                    message = string.Format("ErrorCode: {0}, Message: {1}", responseStatus.Errors[0].ErrorCode, responseStatus.Errors[0].Message);
            }

            switch(logLevel)
            {
                case LogLevel.DEBUG:
                    Log.Debug(title, message, additionalExceptionInfo);
                    break;
                case LogLevel.INFO:
                    Log.Info(title, message, additionalExceptionInfo);
                    break;
                case LogLevel.WARN:
                    Log.Warn(title, message, additionalExceptionInfo);
                    break;
                case LogLevel.ERROR:
                    Log.Error(title, message, additionalExceptionInfo);
                    break;
                case LogLevel.FATAL:
                    Log.Fatal(title, message, additionalExceptionInfo);
                    break;
            }
        }
    }
}