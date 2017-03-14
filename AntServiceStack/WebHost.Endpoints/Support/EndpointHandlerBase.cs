using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Web;
using System.Threading;
using AntServiceStack.Common;
using AntServiceStack.Common.Extensions;
using AntServiceStack.Common.Web;
using Freeway.Logging;
using AntServiceStack.ServiceHost;
using AntServiceStack.ServiceModel.Serialization;
using AntServiceStack.Text;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Types;
using System.Diagnostics;
using AntServiceStack.Common.Hystrix;
using AntServiceStack.Threading;
using HttpRequestExtensions = AntServiceStack.WebHost.Endpoints.Extensions.HttpRequestExtensions;
using HttpRequestWrapper = AntServiceStack.WebHost.Endpoints.Extensions.HttpRequestWrapper;
using HttpResponseWrapper = AntServiceStack.WebHost.Endpoints.Extensions.HttpResponseWrapper;
using System.Threading.Tasks;
using AntServiceStack.WebHost.Endpoints.Utils;
using AntServiceStack.Common.CAT;
using System.Text;
using System.IO;

namespace AntServiceStack.WebHost.Endpoints.Support
{
    public abstract class EndpointHandlerBase
        : IServiceStackHttpHandler, IHttpAsyncHandler, IServiceStackHttpAsyncHandler
    {
        internal static readonly ILog Log = LogManager.GetLogger(typeof(EndpointHandlerBase));

        internal static readonly Dictionary<byte[], byte[]> NetworkInterfaceIpv4Addresses = new Dictionary<byte[], byte[]>();
        internal static readonly byte[][] NetworkInterfaceIpv6Addresses = new byte[0][];

        public string RequestName { get; set; }

        static EndpointHandlerBase()
        {
            try
            {
                IPAddressExtensions.GetAllNetworkInterfaceIpv4Addresses().ForEach((x, y) => NetworkInterfaceIpv4Addresses[x.GetAddressBytes()] = y.GetAddressBytes());

                NetworkInterfaceIpv6Addresses = IPAddressExtensions.GetAllNetworkInterfaceIpv6Addresses().ConvertAll(x => x.GetAddressBytes()).ToArray();
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to retrieve IP Addresses, some security restriction features may not work: " + ex.Message, ex,
                    new Dictionary<string, string>() 
                    {
                        {"ErrorCode", "FXD300062"}
                    });
            }
        }

        public EndpointHandlerBase(string servicePath)
        {
            ServicePath = servicePath;
        }

        public EndpointAttributes HandlerAttributes { get; set; }

        public bool IsReusable
        {
            get { return false; }
        }

        public string ServicePath { get; protected set; }

        public abstract object CreateRequest(IHttpRequest request, string operationName);
        public abstract object GetResponse(IHttpRequest httpReq, IHttpResponse httpRes, object request);

        public virtual void ProcessRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            throw new NotImplementedException();
        }

        public virtual Task ProcessRequestAsync(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            try
            {
                object requestObject = null;
                if (PreProcessRequestAsync(httpReq, httpRes, operationName, out requestObject))
                {
                    tcs.TrySetResult(null);
                    return tcs.Task;
                }

                Task task = GetResponse(httpReq, httpRes, requestObject) as Task;

                string identity = EndpointHost.Config.MetadataMap[httpReq.ServicePath].GetOperationByOpName(httpReq.OperationName).Key;
                return task.ContinueWith(t =>
                {
                    try
                    {
                        object response = null;
                        if (t.Exception != null)
                        {
                            var ex = t.Exception.InnerException ?? t.Exception;
                            if (httpRes != null && httpRes.ExecutionResult != null)
                                httpRes.ExecutionResult.ExceptionCaught = ex;

                            Type responseType = EndpointHost.MetadataMap[ServicePath].GetResponseTypeByOpName(operationName);
                            response = ErrorUtils.CreateServiceErrorResponse(httpReq, ex, responseType);
                            httpRes.WriteToResponse(httpReq, response);
                            return;
                        }

                        response = ((Task<object>)t).Result;
                        if (PostProcessRequestAsync(httpReq, httpRes, operationName, response))
                        {
                            return;
                        }

                        httpRes.WriteToResponse(httpReq, response);
                    }
                    catch (Exception ex)
                    {
                        if (httpRes != null && httpRes.ExecutionResult != null)
                            httpRes.ExecutionResult.ExceptionCaught = ex;

                        throw;
                    }
                    finally
                    {
                        if (EndpointHost.PostResponseFilters.Count > 0)
                        {
                            EndpointHost.ApplyPostResponseFilters(new PostResponseFilterArgs()
                            {
                                ExecutionResult = httpRes.ExecutionResult,
                                ServicePath = httpReq.ServicePath,
                                OperationName = httpReq.OperationName,
                                RequestDeserializeTimeInMilliseconds = httpReq.DeserializationTimeInMillis,
                                ResponseSerializeTimeInMilliseconds = httpRes.SerializationTimeInMillis
                            });
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                try
                {
                    Type responseType = EndpointHost.MetadataMap[ServicePath].GetResponseTypeByOpName(operationName);
                    object response = ErrorUtils.CreateFrameworkErrorResponse(httpReq, ex, responseType);
                    httpRes.WriteToResponse(httpReq, response);
                    tcs.TrySetResult(null);
                }
                catch (Exception ex1)
                {
                    tcs.TrySetException(ex1);
                }

                return tcs.Task;
            }
            finally
            {
                HostContext.Instance.EndRequest();
            }
        }

        protected abstract bool PreProcessRequestAsync(IHttpRequest httpReq, IHttpResponse httpRes, string operationName, out object requestObject);

        protected abstract bool PostProcessRequestAsync(IHttpRequest httpReq, IHttpResponse httpRes, string operationName, object responseObject);

        public static object DeserializeHttpRequest(Type operationType, IHttpRequest httpReq, string contentType)
        {
            var httpMethod = httpReq.HttpMethod;
            var queryString = httpReq.QueryString;

            if (httpMethod == HttpMethods.Get || httpMethod == HttpMethods.Delete || httpMethod == HttpMethods.Options)
            {
                try
                {
                    object request = KeyValueDeserializer.Instance.Parse(queryString, operationType);
                    httpReq.RequestObject = request;
                    return request;
                }
                catch (Exception ex)
                {
                    var msg = "Could not deserialize '{0}' request using KeyValueDeserializer: '{1}'."
                        .Fmt(operationType, queryString);
                    throw new SerializationException(msg, ex);
                }
                finally
                {
                    //httpReq.DeserializationTimeInMillis = deserializeTransaction.Transaction.DurationInMillis;
                }
            }

            var isFormData = httpReq.HasAnyOfContentTypes(ContentType.FormUrlEncoded, ContentType.MultiPartFormData);
            if (isFormData)
            {
                try
                {
                    object request = KeyValueDeserializer.Instance.Parse(httpReq.FormData, operationType);
                    httpReq.RequestObject = request;
                    return request;
                }
                catch (Exception ex)
                {
                    throw new SerializationException("Error deserializing FormData: " + httpReq.FormData, ex);
                }
                finally
                {
                   // httpReq.DeserializationTimeInMillis = deserializeTransaction.Transaction.DurationInMillis;
                }
            }

            return CreateContentTypeRequest(httpReq, operationType, contentType);
        }

        protected static object CreateContentTypeRequest(IHttpRequest httpReq, Type requestType, string contentType)
        {
            var metadata = EndpointHost.MetadataMap[httpReq.ServicePath];
            if (metadata.DeserializeRequestUseMemoryStream)
                return CreateContentTypeRequestUseMemoryStream(httpReq, requestType, contentType);

            if (!string.IsNullOrEmpty(contentType) && httpReq.ContentLength > 0)
            {
                var deserializer = EndpointHost.AppHost.ContentTypeFilters.GetStreamDeserializer(contentType);
                if (deserializer != null)
                {
                    try
                    {
                        object request = deserializer(requestType, httpReq.InputStream);
                        httpReq.RequestObject = request;
                        return request;
                    }
                    catch (Exception ex)
                    {
                        var msg = "Could not deserialize '{0}' request using {1}'\nError: {2}"
                            .Fmt(contentType, requestType, ex);
                        throw new SerializationException(msg);
                    }
                    finally
                    {
                        //httpReq.DeserializationTimeInMillis = deserializeTransaction.Transaction.DurationInMillis;
                    }
                }
            }
            
            object emptyRequest = requestType.CreateInstance(); //Return an empty DTO, even for empty request bodies
            httpReq.RequestObject = emptyRequest;
            return emptyRequest;
        }

        private static object CreateContentTypeRequestUseMemoryStream(IHttpRequest httpReq, Type requestType, string contentType)
        {
            if (!string.IsNullOrEmpty(contentType) && httpReq.ContentLength > 0)
            {
                var deserializer = EndpointHost.AppHost.ContentTypeFilters.GetStreamDeserializer(contentType);
                if (deserializer != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        try
                        {
                            httpReq.InputStream.CopyTo(ms);
                            ms.Flush();
                            ms.Seek(0, SeekOrigin.Begin);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        finally
                        {
                        }

                        try
                        {
                            object request = deserializer(requestType, ms);
                            httpReq.RequestObject = request;
                            return request;
                        }
                        catch (Exception ex)
                        {
                            var msg = "Could not deserialize '{0}' request using {1}'\nError: {2}"
                                .Fmt(contentType, requestType, ex);
                            throw new SerializationException(msg);
                        }
                        finally
                        {
                           // httpReq.DeserializationTimeInMillis = deserializeTransaction.Transaction.DurationInMillis;
                        }
                    }
                }

            }
            
            object emptyRequest = requestType.CreateInstance(); //Return an empty DTO, even for empty request bodies
            httpReq.RequestObject = emptyRequest;
            return emptyRequest;
        }

        protected static object GetCustomRequestFromBinder(IHttpRequest httpReq, Type requestType)
        {
            System.Func<IHttpRequest, object> requestFactoryFn;
            (ServiceManager ?? EndpointHost.ServiceManager).ServiceController.RequestTypeFactoryMap.TryGetValue(
                requestType, out requestFactoryFn);

            return requestFactoryFn != null ? requestFactoryFn(httpReq) : null;
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)
        {
            Debug.Assert(!string.IsNullOrEmpty(this.RequestName));

            AsyncOperation asyncOp = new AsyncOperation(cb, context, extraData, this, ServicePath, RequestName.ToLower());

            if (!asyncOp.IsAsync)
            {
                // Continue tracing if traceId & spanId are provided in request header
                var traceIdString = context.Request.Headers[ServiceUtils.TRACE_ID_HTTP_HEADER] ?? context.Request.Headers[ServiceUtils.ESB_TRACE_ID_HTTP_HEADER];
                if (!string.IsNullOrWhiteSpace(traceIdString))
                    context.Response.AddHeader(ServiceUtils.TRACE_ID_HTTP_HEADER, traceIdString);
                var parentSpanIdString = context.Request.Headers[ServiceUtils.SPAN_ID_HTTP_HEADER] ?? context.Request.Headers[ServiceUtils.ESB_SPAN_ID_HTTP_HEADER];
                if (traceIdString != null && parentSpanIdString != null)
                {
                    try
                    {
                        var traceId = long.Parse(traceIdString);
                        var parentSpanId = long.Parse(parentSpanIdString);
                    }
                    catch (Exception)
                    {
                        // no tracing
                    }
                }

                asyncOp.ContinueCatTransaction();
            }
            else
            {
                HostContext.Instance.Request.Items[ServiceCatConstants.SOA2AsyncServiceStartTimeKey] = DateTime.Now;
            }

            try
            {
                //检查单个IP的连接数是否超标
                CheckConnectionMaxRequestCount(context);
                //电容保护针对每个方法
                var hystrixCommand = EndpointHost.Config.MetadataMap[ServicePath].GetHystrixCommandByOpName(this.RequestName);
                var hystrixMetrics = hystrixCommand.Metrics;

                var hystrixCircuitBreaker = hystrixCommand.CircuitBreaker;
                // We fallback if circuit is open
                if (!hystrixCircuitBreaker.AllowRequest())
                {
                    hystrixMetrics.MarkShortCircuited();
                    //设置_slaErrorSent 
                    asyncOp.SendSyncSLAErrorResponse(HystrixEventType.ShortCircuited);
                    if (asyncOp.IsAsync)
                    {
                       // var message = "Circuit Breaker is open. Server entered into self-protecting mode";
                    }
                    return asyncOp;
                }

                if (asyncOp.IsAsync)
                {
                    // The circuit is healthy, We can start async work
                    hystrixMetrics.IncrementConcurrentExecutionCount();
                    EndpointHost.Config.MetadataMap[ServicePath].IncrementConcurrentExecutionCount();
                    asyncOp.StartAsyncWork();
                }
                else // sync mode
                {
                    asyncOp.StartSyncWork();
                }

                return asyncOp;
            }
            catch (Exception)
            {
                throw;
            }
            finally 
            {
            }
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            var asyncOp = result as AsyncOperation;

            // Calculate circuit health
            var hystrixCommand = EndpointHost.Config.MetadataMap[ServicePath].GetHystrixCommandByOpName(this.RequestName);
            var hystrixMetrics = hystrixCommand.Metrics;
            var hystrixCommandProperties = hystrixCommand.Properties;
            var hystrixCircuitBreaker = hystrixCommand.CircuitBreaker;

            if (asyncOp.IsAsync)
            {
                hystrixMetrics.DecrementConcurrentExecutionCount();
                EndpointHost.Config.MetadataMap[ServicePath].DecrementConcurrentExecutionCount();
            }
            else
            {

                asyncOp.CompleteCatTransaction();
            }

            // No need to do additional circuit health calculation since if SLA error has been sent,
            // the circuit health calculation has already been done in BeginProcessRequest.
            if (asyncOp.SLAErrorSent)
            {
                return;
            }

            DateTime now = DateTime.Now;
            TimeSpan totalExecutionTime = now - asyncOp.StartTime;
            bool isTimeout = false;
            TimeSpan timeout = hystrixCommand.GetExecutionTimeout();
            if (totalExecutionTime > timeout)
            {
                isTimeout = true;
                hystrixMetrics.MarkTimeout((long)totalExecutionTime.TotalMilliseconds);
                Dictionary<string, string> addtionalData = new Dictionary<string, string>();
                addtionalData.Add("Service", EndpointHost.Config.MetadataMap[ServicePath].FullServiceName);
                addtionalData.Add("Operation", this.RequestName);
                addtionalData.Add("ErrorCode", "FXD300070");
                string title = string.Format("Service execution is too long: {0} ms", (long)totalExecutionTime.TotalMilliseconds);
                Log.Warn(title, addtionalData);
            }

            hystrixMetrics.AddTotalExecutionTime(Convert.ToInt64(totalExecutionTime.TotalMilliseconds));

            var executionResult = asyncOp.ExecutionResult;
            hystrixMetrics.AddServiceExecutionTime(executionResult.ServiceExecutionTime);

            if (!isTimeout) // we don't care exception if timeout happened
            {
                //.net内部出错
                if (executionResult.FrameworkExceptionThrown)
                {
                    hystrixMetrics.MarkFrameworkExceptionThrown();
                }
                //执行soa接口出错
                else if (executionResult.ServiceExceptionThrown)
                {
                    hystrixMetrics.MarkServiceExceptionThrown();
                }
                //验证不通过 包括 要求auth但没有 白名单 黑名单之类
                else if (executionResult.ValidationExceptionThrown)
                {
                    hystrixMetrics.MarkValidationExceptionThrown();
                    // validation exception is not severe error,
                    // We let the cricuit-breaker mark a success
                    hystrixCircuitBreaker.MarkSuccess();
                }
                else
                {
                    hystrixMetrics.MarkSuccess((long)totalExecutionTime.TotalMilliseconds);
                    hystrixCircuitBreaker.MarkSuccess();
                }
            }
        }

        // invalid for asnyc handler
        public virtual void ProcessRequest(HttpContext context)
        {
            throw new InvalidOperationException();
        }

        public static ServiceManager ServiceManager { get; set; }

        public static Type GetRequestType(string operationName, string servicePath)
        {
            return ServiceManager != null ? ServiceManager.MetadataMap[servicePath].GetRequestTypeByOpName(operationName)
                : EndpointHost.Config.MetadataMap[servicePath].GetRequestTypeByOpName(operationName);
        }

        protected static object ExecuteService(object request, EndpointAttributes endpointAttributes,
            IHttpRequest httpReq, IHttpResponse httpRes)
        {
            return EndpointHost.ExecuteService(request, endpointAttributes, httpReq, httpRes);
        }

        protected static void AssertOperationExists(string operationName, Type type)
        {
            if (type == null)
            {
                throw new NotImplementedException(
                    string.Format("The operation '{0}' does not exist for this service", operationName));
            }
        }

        protected void HandleException(IHttpRequest httpReq, IHttpResponse httpRes, string operationName, Exception ex, bool throwException = true)
        {
            try
            {
                EndpointHost.ExceptionHandler(httpReq, httpRes, ex);
            }
            catch (Exception writeErrorEx)
            {
                Log.Info("Failed to write error to response", writeErrorEx, new Dictionary<string, string>() { { "ErrorCode", "FXD300063" } });
                var errorMessage = string.Format("Error occured while Processing Request: {0}", ex.Message);
                Log.Error(errorMessage, ex, new Dictionary<string, string>() 
                { 
                    { "Version", ServiceUtils.SOA2VersionCatName},
                    { "ErrorCode", "FXD300000" },
                    { "Service", EndpointHost.MetadataMap[httpReq.ServicePath].FullServiceName},
                    { "Operation", operationName},
                    { "ErrorClassification", "FrameworkError"}
                });

                if(throwException)
                    throw ex;
            }
            finally
            {
                if (!httpRes.IsClosed)
                    httpRes.AddHeader(ServiceUtils.ResponseStatusHttpHeaderKey, AckCodeType.Failure.ToString());
                httpRes.LogRequest(httpReq);
                httpRes.EndRequest(true);
            }
        }

        protected void HandleServiceException(IHttpRequest httpReq, IHttpResponse httpRes, string operationName, Exception ex, bool throwException = true)
        {
            try
            {
                Operation operation = EndpointHost.MetadataMap[httpReq.ServicePath].OperationNameMap[httpReq.OperationName.ToLower()];

                httpRes.ResponseObject = ErrorUtils.CreateServiceErrorResponse(httpReq, ex, operation.ResponseType);
                httpRes.WriteToResponse(httpReq, httpRes.ResponseObject);
            }
            catch
            {
                var errorMessage = string.Format("Error occured while Processing Request: {0}", ex.Message);
                Log.Error(errorMessage, ex, new Dictionary<string, string>() 
                { 
                    { "Version", ServiceUtils.SOA2VersionCatName},
                    { "ErrorCode", "FXD300000" },
                    { "Service", EndpointHost.MetadataMap[httpReq.ServicePath].FullServiceName},
                    { "Operation", operationName},
                    { "ErrorClassification", "FrameworkError"}
                });

                if (throwException)
                    throw ex;
            }
            finally
            {
                if (!httpRes.IsClosed)
                    httpRes.AddHeader(ServiceUtils.ResponseStatusHttpHeaderKey, AckCodeType.Failure.ToString());
                httpRes.LogRequest(httpReq);
                httpRes.EndRequest(true);
            }
        }

        /// <summary>
        /// 检查单个IP的连接数是否超标
        /// </summary>
        /// <param name="context"></param>
        private void CheckConnectionMaxRequestCount(HttpContext context)
        {
            try
            {
                string remoteIP = null;
                string remotePort = null;
                if (string.Equals(context.Request.Headers[HttpHeaders.Connection],
                    HttpHeaders.HeaderValues.ConnectionClose, StringComparison.InvariantCultureIgnoreCase))
                {
                    remoteIP = context.Request.ServerVariables["REMOTE_HOST"];
                    remotePort = context.Request.ServerVariables["REMOTE_PORT"];
                    Log.Info(string.Format("Client{{{0}:{1}}} sent Connection:close to the service to close the connection after this request.",
                        remoteIP, remotePort), 
                        new Dictionary<string, string>()
                        {
                            { "ErrorCode", "FXD300080" },
                            { "ClientIP", remoteIP },
                            { "ClientAppId", context.Request.GetClientAppId() }
                        });
                }


                if (!EndpointHost.Config.MetadataMap[ServicePath].CheckConnectionMaxRequestCount)
                    return;

                remoteIP = remoteIP ?? context.Request.ServerVariables["REMOTE_HOST"];
                remotePort = remotePort ?? context.Request.ServerVariables["REMOTE_PORT"];
                int requestCount = ConnectionRequestCounterCache.Instance.IncrementAndGet(remoteIP, remotePort);
                if (requestCount > EndpointHost.Config.MetadataMap[ServicePath].ConnectionMaxRequestCount)
                {
                    ConnectionRequestCounterCache.Instance.Reset(remoteIP, remotePort);
                    context.Response.AddHeader(HttpHeaders.Connection, HttpHeaders.HeaderValues.ConnectionClose);
                    Log.Info(string.Format("Service sent Connection:close to the client{{{0}:{1}}} to close the connection after this request.",
                        remoteIP, remotePort), 
                        new Dictionary<string, string>()
                        {
                            { "ErrorCode", "FXD300081" },
                            { "ClientIP", remoteIP },
                            { "ClientAppId", context.Request.GetClientAppId() }
                        });
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to check max request count", ex, new Dictionary<string, string>() { { "ErrorCode", "FXD300074" } });
            }
        }

        public virtual void SendSyncSLAErrorResponse(IHttpRequest request, IHttpResponse response, HystrixEventType hystrixEvent)
        {
            Type responseType = EndpointHost.Config.MetadataMap[ServicePath].GetResponseTypeByOpName(request.OperationName);
            var errorResponse = ErrorUtils.CreateSLAErrorResponse(hystrixEvent, responseType);
            response.WriteToResponse(request, errorResponse);
        }
    }
}