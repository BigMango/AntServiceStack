using System;
using System.IO;
using System.Net;
using System.Web;
using AntServiceStack.Common.Web;
using AntServiceStack.Common.Types;
using Freeway.Logging;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.CAT;

namespace AntServiceStack.WebHost.Endpoints.Extensions
{
    public static class HttpResponseExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpResponseExtensions));

        public static bool WriteToResponse(this IHttpResponse httpRes, IHttpRequest httpReq, object result, string contentType)
        {
            var serializer = EndpointHost.AppHost.ContentTypeFilters.GetResponseSerializer(contentType);
            return httpRes.WriteToResponse(httpReq, result, serializer, new SerializationContext(contentType), null, null);
        }

        public static bool WriteToResponse(this IHttpResponse httpRes, IHttpRequest httpReq, object result)
        {
            return WriteToResponse(httpRes, httpReq, result, null, null);
        }

        public static bool WriteToResponse(this IHttpResponse httpRes, IHttpRequest httpReq, object result, byte[] bodyPrefix, byte[] bodySuffix)
        {
            if (result == null)
            {
                httpRes.LogRequest(httpReq);
                httpRes.EndRequestWithNoContent();
                return true;
            }

            var serializationContext = new HttpRequestContext(httpReq, httpRes, result);

            var serializer = EndpointHost.AppHost.ContentTypeFilters.GetResponseSerializer(httpReq.ResponseContentType);
            return httpRes.WriteToResponse(httpReq, result, serializer, serializationContext, bodyPrefix, bodySuffix);
        }

        public static bool WriteToResponse(this IHttpResponse httpRes, IHttpRequest httpReq, object result, ResponseSerializerDelegate serializer, byte[] bodyPrefix, byte[] bodySuffix)
        {
            if (result == null)
            {
                httpRes.LogRequest(httpReq);
                httpRes.EndRequestWithNoContent();
                return true;
            }

            var serializationContext = new HttpRequestContext(httpReq, httpRes, result);

            return httpRes.WriteToResponse(httpReq, result, serializer, serializationContext, bodyPrefix, bodySuffix);
        }

        /// <summary>
        /// Writes to response.
        /// Response headers are customizable by implementing IHasOptions an returning Dictionary of Http headers.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="result">Whether or not it was implicity handled by ServiceStack's built-in handlers.</param>
        /// <param name="defaultAction">The default action.</param>
        /// <param name="serializerCtx">The serialization context.</param>
        /// <param name="bodyPrefix">Add prefix to response body if any</param>
        /// <param name="bodySuffix">Add suffix to response body if any</param>
        /// <returns></returns>
        public static bool WriteToResponse(this IHttpResponse response, IHttpRequest request, object result, ResponseSerializerDelegate defaultAction, IRequestContext serializerCtx, byte[] bodyPrefix, byte[] bodySuffix)
        {
            var defaultContentType = serializerCtx.ResponseContentType;
            AckCodeType ack = AckCodeType.Success;
            bool completed = true;
            try
            {
                if (result == null)
                {
                    response.EndRequestWithNoContent();
                    return true;
                }

                ApplyGlobalResponseHeaders(response);

                /* Mono Error: Exception: Method not found: 'System.Web.HttpResponse.get_Headers' */
                var responseOptions = result as IHasOptions;
                if (responseOptions != null)
                {
                    //Reserving options with keys in the format 'xx.xxx' (No Http headers contain a '.' so its a safe restriction)
                    const string reservedOptions = ".";

                    foreach (var responseHeaders in responseOptions.Options)
                    {
                        if (responseHeaders.Key.Contains(reservedOptions)) continue;

                        Log.Debug(string.Format("Setting Custom HTTP Header: {0}: {1}", responseHeaders.Key, responseHeaders.Value),
                            new Dictionary<string, string>() 
                            {
                                {"ErrorCode", "FXD300061"}
                            });
                        response.AddHeader(responseHeaders.Key, responseHeaders.Value);
                    }
                }

                var disposableResult = result as IDisposable;

                //ContentType='text/html' is the default for a HttpResponse
                //Do not override if another has been set
                if (response.ContentType == null || response.ContentType == ContentType.Html)
                {
                    response.ContentType = defaultContentType;
                }
                if (bodyPrefix != null && response.ContentType.IndexOf(ContentType.Json, StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    response.ContentType = ContentType.JavaScript;
                }

                if (EndpointHost.Config.AppendUtf8CharsetOnContentTypes.Contains(response.ContentType))
                {
                    response.ContentType += ContentType.Utf8Suffix;
                }

                var responseText = result as string;
                if (responseText != null)
                {
                    if (bodyPrefix != null) response.OutputStream.Write(bodyPrefix, 0, bodyPrefix.Length);
                    WriteTextToResponse(response, responseText, defaultContentType);
                    if (bodySuffix != null) response.OutputStream.Write(bodySuffix, 0, bodySuffix.Length);
                    return true;
                }

                var commonResponseDto = result as IHasResponseStatus;
                if (commonResponseDto != null)
                {
                    // defensive programming
                    if (commonResponseDto.ResponseStatus == null)
                    {
                        commonResponseDto.ResponseStatus = new ResponseStatusType();
                    }
                    commonResponseDto.ResponseStatus.Timestamp = DateTime.Now;
                    // TODO add version

                    // post ack check, in case developer forget to set ack according to error status 
                    bool hasError = false;
                    if (commonResponseDto.ResponseStatus.Ack == AckCodeType.Success && commonResponseDto.ResponseStatus.Errors.Count > 0)
                    {
                        foreach (ErrorDataType error in commonResponseDto.ResponseStatus.Errors)
                        {
                            if (error.SeverityCode == SeverityCodeType.Error)
                            {
                                hasError = true;
                                break;
                            }
                        }
                        if (hasError)
                        {
                            commonResponseDto.ResponseStatus.Ack = AckCodeType.Failure;
                        }
                    }

                    ack = commonResponseDto.ResponseStatus.Ack;

                    AddRequestInfoToResponseStatus(serializerCtx.Get<IHttpRequest>(), commonResponseDto);
                }

                // Defensive programming, in normal case, we should not see GenericErrorResponseType here
                // In case any exception, we set http status code to trigger SOA C# client side WebServiceException
                var genericErrorResponseDto = result as GenericErrorResponseType;
                if (genericErrorResponseDto != null)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    ack = AckCodeType.Failure;
                }

                if (defaultAction == null)
                {
                    throw new ArgumentNullException("defaultAction", String.Format(
                    "As result '{0}' is not a supported responseType, a defaultAction must be supplied",
                    (result != null ? result.GetType().Name : "")));
                }

                if (EndpointHost.Config.ServiceManager.MetadataMap[request.ServicePath].UseChunkedTransferEncoding)
                {
                    response.AddHeader(ServiceUtils.ResponseStatusHttpHeaderKey, ack.ToString());
                    response.UseChunkedTransferEncoding();
                }

                if (bodyPrefix != null) response.OutputStream.Write(bodyPrefix, 0, bodyPrefix.Length);
                if (result != null)
                {
                    try
                    {
                        defaultAction(serializerCtx, result, response);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                       //response.SerializationTimeInMillis = serializeTransaction.Transaction.DurationInMillis;
                    }
                }
                if (bodySuffix != null) response.OutputStream.Write(bodySuffix, 0, bodySuffix.Length);
                
                // Record response size
                response.ExecutionResult.ResponseSize = response.OutputStream.Length;

                if (disposableResult != null) disposableResult.Dispose();

                return false;
            }
            catch (Exception originalEx)
            {
                ack = AckCodeType.Failure;

                bool usedChunkedTransferEncoding = response.UsedChunkedTransferEncoding();
                var errorMessage = string.Format("Error occured while {0}: [{1}] {2}",
                    usedChunkedTransferEncoding ? "using chunked transfer encoding" : "processing request", originalEx.GetType().Name, originalEx.Message);
                Log.Error(errorMessage, originalEx, new Dictionary<string, string>(){ { "ErrorCode", "FXD300010" } });

                //TM: It would be good to handle 'remote end dropped connection' problems here. Arguably they should at least be suppressible via configuration

                //DB: Using standard ServiceStack configuration method

                if (!EndpointHost.Config.WriteErrorsToResponse)
                {
                    completed = false;
                    throw;
                }

                if (response.IsClosed)
                    return true;
                
                if (usedChunkedTransferEncoding)
                    return true;

                try
                {
                    response.WriteErrorToResponse(serializerCtx.Get<IHttpRequest>(), defaultContentType, originalEx);
                    return true;
                }
                catch (Exception writeErrorEx)
                {
                    //Exception in writing to response should not hide the original exception
                    Log.Error("Failed to write error to response: " + writeErrorEx.Message, writeErrorEx, new Dictionary<string, string>(){ { "ErrorCode", "FXD300010" } });
                    completed = false;
                    throw originalEx;
                }
            }
            finally
            {
                if (!response.UsedChunkedTransferEncoding())
                    response.AddHeader(ServiceUtils.ResponseStatusHttpHeaderKey, ack.ToString());

                if (completed)
                    response.LogRequest(request);

                response.EndRequest(true);
            }
        }

        public static void WriteTextToResponse(this IHttpResponse response, string text, string defaultContentType)
        {
            try
            {
                //ContentType='text/html' is the default for a HttpResponse
                //Do not override if another has been set
                if (response.ContentType == null || response.ContentType == ContentType.Html)
                {
                    response.ContentType = defaultContentType;
                }

                response.Write(text);
            }
            catch (Exception ex)
            {
                Log.Error("Could not WriteTextToResponse: " + ex.Message, ex, new Dictionary<string, string>{ { "ErrorCode", "FXD300010" } });
                throw;
            }
        }

        public static void WriteErrorToResponse(this IHttpResponse httpRes, IHttpRequest httpReq, string contentType, Exception ex)
        {
            WriteErrorToResponse(httpRes, httpReq, contentType, ex, true);
        }

        public static void WriteErrorToResponse(this IHttpResponse httpRes, IHttpRequest httpReq, string contentType, Exception ex, bool isCritical)
        {
            WriteErrorToResponse(httpRes, httpReq, contentType, ex, isCritical, null);
        }

        public static void WriteErrorToResponse(this IHttpResponse httpRes, IHttpRequest httpReq,
            string contentType, Exception ex, bool isCritical, string errorCode)
        {
            // Mark exception was thrown
            if (httpRes.ExecutionResult != null)
            {
                if (httpReq.RequestObject == null)
                    httpRes.ExecutionResult.ValidationExceptionThrown = true;
                else
                    httpRes.ExecutionResult.FrameworkExceptionThrown = true;
            }

            string operationName = httpReq.OperationName;
            var responseType = operationName == null ? null : EndpointHost.Config.MetadataMap[httpReq.ServicePath].GetResponseTypeByOpName(operationName);
            var errorResponseDto = ErrorUtils.CreateFrameworkErrorResponse(httpReq, ex, responseType, isCritical, errorCode);

            if (httpRes.ContentType == null || httpRes.ContentType == ContentType.Html)
            {
                httpRes.ContentType = contentType;
            }
            if (EndpointHost.Config.AppendUtf8CharsetOnContentTypes.Contains(contentType))
            {
                httpRes.ContentType += ContentType.Utf8Suffix;
            }

            AddRequestInfoToResponseStatus(httpReq, errorResponseDto as IHasResponseStatus);

            if (errorResponseDto is GenericErrorResponseType)
            {
                // With no response type specified, 
                // AntServiceStack C# client should throw WebProtocolException with a generic error response dto,
                // We use http status code here to trigger client side WebServiceException.
                httpRes.StatusCode = ex.ToStatusCode(); 
            }
            var serializationCtx = new SerializationContext(contentType);

            var serializer = EndpointHost.AppHost.ContentTypeFilters.GetResponseSerializer(contentType);
            if (serializer != null)
            {
                try
                {
                    serializer(serializationCtx, errorResponseDto, httpRes);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    //httpRes.SerializationTimeInMillis = serializeTransaction.Transaction.DurationInMillis;
                }
            }

            // Record error response size
            if (httpRes.ExecutionResult != null)
            {
                httpRes.ExecutionResult.ResponseSize = httpRes.OutputStream.Length;
            }

            // outer caller will close the response
            //httpRes.EndHttpHandlerRequest(skipHeaders: true);
        }

        public static int ToStatusCode(this Exception ex)
        {
            if (ex is NotImplementedException || ex is NotSupportedException) return (int)HttpStatusCode.MethodNotAllowed;
            if (ex is ArgumentException || ex is SerializationException) return (int)HttpStatusCode.BadRequest;
            if (ex is UnauthorizedAccessException) return (int)HttpStatusCode.Forbidden;
            return (int)HttpStatusCode.InternalServerError;
        }

        public static void ApplyGlobalResponseHeaders(this HttpListenerResponse httpRes)
        {
            if (EndpointHost.Config == null) return;
            foreach (var globalResponseHeader in EndpointHost.Config.GlobalResponseHeaders)
            {
                httpRes.AddHeader(globalResponseHeader.Key, globalResponseHeader.Value);
            }
        }

        public static void ApplyGlobalResponseHeaders(this HttpResponse httpRes)
        {
            if (EndpointHost.Config == null) return;
            foreach (var globalResponseHeader in EndpointHost.Config.GlobalResponseHeaders)
            {
                httpRes.AddHeader(globalResponseHeader.Key, globalResponseHeader.Value);
            }
        }

        public static void ApplyGlobalResponseHeaders(this IHttpResponse httpRes)
        {
            if (EndpointHost.Config == null) return;
            foreach (var globalResponseHeader in EndpointHost.Config.GlobalResponseHeaders)
            {
                httpRes.AddHeader(globalResponseHeader.Key, globalResponseHeader.Value);
            }
        }

        private static void AddRequestInfoToResponseStatus(IHttpRequest httpReq, IHasResponseStatus responseObject)
        {
            if (httpReq == null || responseObject == null)
                return;

            var traceIdString = httpReq.Headers[ServiceUtils.TRACE_ID_HTTP_HEADER];
            if (!string.IsNullOrWhiteSpace(traceIdString))
                responseObject.AddExtensionData(ServiceUtils.TRACE_ID_HTTP_HEADER, traceIdString);

            IHasMobileRequestHead mobileRequest = httpReq.RequestObject as IHasMobileRequestHead;
            if (mobileRequest != null)
            {
                foreach (string extensionKey in ServiceUtils.MobileWriteBackExtensionKeys)
                {
                    string extensionData = mobileRequest.GetExtensionData(extensionKey);
                    if (extensionData != null)
                        responseObject.AddExtensionData(extensionKey, extensionData);
                }
            }

            if (httpReq.IsH5GatewayRequest())
            {
                foreach (string key in httpReq.Headers.Keys)
                {
                    string refinedKey = key.ToLower();
                    if (refinedKey.StartsWith(ServiceUtils.H5GatewayResponseDataHeaderPrefix))
                    {
                        string value = httpReq.Headers[key];
                        refinedKey = key.Substring(ServiceUtils.H5GatewayResponseDataHeaderPrefix.Length);
                        if (!string.IsNullOrWhiteSpace(refinedKey))
                            responseObject.AddExtensionData(refinedKey, value);
                    }
                }
            }
        }

        public static void UseChunkedTransferEncoding(this IHttpResponse httpRes)
        {
            HttpResponse httpResponse = httpRes.OriginalResponse as HttpResponse;
            if (httpResponse != null)
            {
                httpResponse.BufferOutput = false;
                return;
            }

            HttpListenerResponse httpListenerResponse = httpRes.OriginalResponse as HttpListenerResponse;
            if (httpListenerResponse != null)
            {
                httpListenerResponse.SendChunked = true;
                return;
            }
        }

        public static bool UsedChunkedTransferEncoding(this IHttpResponse httpRes)
        {
            HttpResponse httpResponse = httpRes.OriginalResponse as HttpResponse;
            if (httpResponse != null)
                return !httpResponse.BufferOutput;

            HttpListenerResponse httpListenerResponse = httpRes.OriginalResponse as HttpListenerResponse;
            if (httpListenerResponse != null)
                return httpListenerResponse.SendChunked;

            return false;
        }

        [Obsolete("Use EndRequest extension method")]
        public static void EndServiceStackRequest(this IHttpResponse httpRes, bool skipHeaders = false)
        {
            httpRes.EndRequest(skipHeaders);
        }
    }
}
