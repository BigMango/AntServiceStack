using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Concurrent;

using Freeway.Logging;
using AntServiceStack.Text;
using AntServiceStack.Text.Jsv;
using AntServiceStack.Common.Web;
using AntServiceStack.Common.Extensions;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Types;
using AntServiceStack.Common.Configuration;
using AntServiceStack.Common.Execution;
using ExecutionContext = AntServiceStack.Common.Execution.ExecutionContext;
using AntServiceStack.ServiceHost;
using AntServiceStack.Client.CHystrix;
using AntServiceStack.Client.CAT;
using AntServiceStack.Common.CAT;
using AntServiceStack.Client.ServiceClient;

namespace AntServiceStack.ServiceClient
{
    public abstract partial class ServiceClientBase<DerivedClient> : ServiceClientBase
        where DerivedClient : ServiceClientBase<DerivedClient> 
    {
        private TResponse Invoke<TResponse>(ClientExecutionContext context)
        {
            Stopwatch stopwatch = null;
            Stopwatch remoteCallStopwatch = null;

            try
            {
                stopwatch = new Stopwatch();

                stopwatch.Start();

                ApplyRequestFilters(ServiceName, ServiceNamespace, context.Operation, context.Request);

                HttpWebRequest webRequest = PrepareWebRequest(context);

                remoteCallStopwatch = new Stopwatch();
                remoteCallStopwatch.Start();
                TResponse response;
                using (WebResponse webResponse = Invoke(context, webRequest))
                {
                    remoteCallStopwatch.Stop();
                    context.Metrics.ResponseSize = webResponse.ContentLength < 0 ? 0 : webResponse.ContentLength;


                    response = HandleResponse<TResponse>(context, webResponse);
                }

                ApplyResponseFilters(ServiceName, ServiceNamespace, context.Operation, context.Response);


                context.IsSuccess = true;
                return response;
            }
            catch (CServiceException cex)
            {
                Dictionary<string, string> additionalInfo = GetClientInfo(context);
                additionalInfo["ErrorCode"] = "FXD301004";
                if (LogErrorWithRequestInfo)
                    additionalInfo["RequestObject"] = TypeSerializer.SerializeToString(context.Request);
                LogCServiceException(GetLogTitle(InternalServiceUtils.ServiceErrorTitle), cex, additionalInfo);


                throw;
            }
            catch (WebException ex)
            {
                WebException subClassedException = HandleWebException(context, (WebException)ex);

                LogGeneralException(context, subClassedException);

                if (ex == subClassedException)
                    throw;

                throw subClassedException;
            }
            catch (Exception ex)
            {
                LogGeneralException(context, ex);

                throw;
            }
            finally
            {


                if (remoteCallStopwatch != null)
                {
                    if(remoteCallStopwatch.IsRunning)
                        remoteCallStopwatch.Stop();
                    context.Metrics.ExecutionTime = remoteCallStopwatch.ElapsedMilliseconds;
                }

                stopwatch.Stop();
                context.Metrics.TotalTime = stopwatch.ElapsedMilliseconds;

                ResetCurrentRequestTimeoutSetting();
            }
        }

        /// <summary>
        /// 调用目标服务
        /// </summary>
        /// <typeparam name="TResponse">服务响应</typeparam>
        /// <param name="operationName">目标服务操作名</param>
        /// <param name="requestObject">服务请求对象</param>
        /// <returns>服务调用响应</returns>
        public TResponse Invoke<TResponse>(string operationName, object requestObject)
        {
            return Invoke<TResponse>(operationName, requestObject, null);
        }

        public TResponse Invoke<TResponse>(string operationName, object requestObject, Func<TResponse> getFallback)
        {
            ClientExecutionContext context = CreateExecutionContext(operationName, requestObject, ExecutionModes.SynchronizationMode);
            Validate(context);

            try
            {
                if (!EnableCHystrixSupport)
                    return InvokeInternal<TResponse>(context);

                string chystrixCommandKey;
                string chystrixInstanceKey;
                GetCHystrixCommandKey(context, out chystrixCommandKey, out chystrixInstanceKey);

                if (getFallback == null)
                    return CHystrixIntegration.RunCommand<TResponse>(chystrixInstanceKey, chystrixCommandKey, () => InvokeInternal<TResponse>(context), null);

                return CHystrixIntegration.RunCommand<TResponse>(chystrixInstanceKey, chystrixCommandKey, () => InvokeInternal<TResponse>(context), () => getFallback());
            }
            catch (Exception ex)
            {
                context.IsSuccess = false;
                context.Error = ex;

                throw;
            }
            finally
            {
                ApplyRequestEndFilterSafe(context);
            }
        }

        private TResponse InvokeInternal<TResponse>(ClientExecutionContext context)
        {
            return Invoke<TResponse>(context);
        }

        /// <summary>
        /// 创建异步请求Task，非IOCP实现
        /// </summary>
        /// <typeparam name="TRequest">服务请求</typeparam>
        /// <typeparam name="TResponse">服务响应</typeparam>
        /// <param name="operationName">目标服务操作名</param>
        /// <param name="requestObject">服务请求对象</param>
        /// <param name="cancellationToken"></param>
        /// <param name="taskCreationOptions"></param>
        /// <returns>异步请求Task</returns>
        public Task<TResponse> CreateAsyncTask<TRequest, TResponse>(string operationName, object requestObject, CancellationToken? cancellationToken = null, TaskCreationOptions? taskCreationOptions = null)
        {
           
            ClientExecutionContext context = CreateExecutionContext(operationName, requestObject, ExecutionModes.AsyncMode);

            TimeSpan? timeout = CurrentRequestTimeout;
            TimeSpan? readWriteTimeout = CurrentRequestReadWriteTimeout;
            ResetCurrentRequestTimeoutSetting();

            Func<TResponse> sendWebRequest = () =>
            {
                try
                {
                    InitThreadLocalBaseUri(context);
                    if (timeout.HasValue)
                        CurrentRequestTimeout = timeout.Value;
                    if (readWriteTimeout.HasValue)
                        CurrentRequestReadWriteTimeout = readWriteTimeout.Value;

                    return Invoke<TResponse>(context);
                }
                catch (Exception ex)
                {
                    context.IsSuccess = false;
                    context.Error = ex;

                    throw;
                }
                finally
                {
                    ApplyRequestEndFilterSafe(context);
                }
            };
            Task<TResponse> task = null;
            if (!cancellationToken.HasValue && !taskCreationOptions.HasValue)
                task = new Task<TResponse>(sendWebRequest);
            else if (cancellationToken.HasValue && !taskCreationOptions.HasValue)
                task = new Task<TResponse>(sendWebRequest, cancellationToken.Value);
            else if (!cancellationToken.HasValue && taskCreationOptions.HasValue)
                task = new Task<TResponse>(sendWebRequest, taskCreationOptions.Value);
            else
                task = new Task<TResponse>(sendWebRequest, cancellationToken.Value, taskCreationOptions.Value);

            return TryAddFaultHandler(task);
        }

        private void CheckResponseFailure(IHasResponseStatus response)
        {
            if (response == null)
                return; // throw new Exception("Generated client code doesn't have the response type inherit from IHasReponseStatus interface.");

            if (response.ResponseStatus == null)
                throw new CServiceException("Generated service code doesn't have the response type inherit from IHasReponseStatus interface.");

            if (response.ResponseStatus.Ack == AckCodeType.Failure)
            {
                var ex = ExceptionFactory.CreateCServiceException(response.ResponseStatus.Errors);
                ex.ResponseObject = response;
                throw ex;
            }
        }

        private WebResponse Invoke(ClientExecutionContext context, WebRequest request)
        {

            try
            {
                WebResponse response = request.GetResponse();
                ServerAvailabilityChecker.CheckServerAvailability(context);
                return response;
            }
            catch (Exception ex)
            {
                ServerAvailabilityChecker.CheckServerAvailability(context, ex);
                throw;
            }
        }

        protected internal WebException HandleWebException(ClientExecutionContext context, WebException webEx)
        {
            using (webEx.Response)
            {
                HttpWebResponse errorResponse = webEx.Response as HttpWebResponse;
                if (errorResponse == null)
                {
                    if (webEx is WebExceptionBase)
                        return webEx;
                    return ExceptionFactory.CreateWebException(webEx);
                }

                if (webEx.Status == WebExceptionStatus.ProtocolError)
                {
                    string title = string.IsNullOrWhiteSpace(errorResponse.StatusDescription) ? webEx.Message : errorResponse.StatusDescription;

                    WebProtocolException serviceEx = null;
                    if (webEx is WebProtocolException)
                        serviceEx = (WebProtocolException)webEx;
                    else
                        serviceEx = (WebProtocolException)ExceptionFactory.CreateWebException(webEx, title);

                    try
                    {
                        using (Stream stream = errorResponse.GetResponseStream())
                        {
                            if (errorResponse.ContentType.MatchesContentType(context.ContentType))
                            {
                                var bytes = stream.ReadFully();
                                using (var memoryStream = new MemoryStream(bytes))
                                {
                                    serviceEx.ResponseBody = bytes.FromUtf8Bytes();
                                    serviceEx.ResponseObject = (GenericErrorResponseType)context.CallFormat.StreamDeserializer(typeof(GenericErrorResponseType), memoryStream);
                                }
                            }
                            else
                            {
                                serviceEx.ResponseBody = stream.ToUtf8String();
                            }
                        }
                    }
                    catch (Exception innerEx)
                    {
                        // Oh, well, we tried
                        return new WebProtocolException(title, innerEx)
                        {
                            StatusCode = (int)errorResponse.StatusCode,
                            StatusDescription = errorResponse.StatusDescription,
                            ResponseBody = serviceEx.ResponseBody
                        };
                    }

                    //Escape deserialize exception handling and throw here
                    return serviceEx;
                }

                return webEx;
            }
        }

        private void Validate(ClientExecutionContext context)
        {
            Exception ex = null;
            if (string.IsNullOrWhiteSpace(context.Format))
                ex = new ArgumentNullException("Format was not supplied");
            else if (CurrentCallFormat == null)
                ex = new NotSupportedException(string.Format("Not supported call format : {0}", context.Format));
            else if (string.IsNullOrWhiteSpace(context.ServiceUrl) || !(context.ServiceUrl.StartsWith("http://") || context.ServiceUrl.StartsWith("https://")))
                ex = new NotSupportedException(string.Format("BaseUri was not supplied or invalid : {0}", context.ServiceUrl));
            else if (string.IsNullOrWhiteSpace(context.Operation))
                ex = new ArgumentNullException("Operation name was not supplied");
            else if (context.Request == null)
                ex = new ArgumentNullException("Request object was not supplied");

            if (ex != null)
            {
                LogGeneralException(context, ex);
                throw ex;
            }
        }

        private HttpWebRequest PrepareWebRequest(ClientExecutionContext context)
        {
            NameValueCollection addHeaders = null;

            HttpWebRequest client = PrepareWebRequest(context, addHeaders);

            using (var requestStream = client.GetRequestStream())
            {
                Stopwatch serializationWatch = new Stopwatch();
                try
                {
                    serializationWatch.Start();
                    context.CallFormat.StreamSerializer(context.Request, requestStream);
                }
                catch (WebException ex)
                {
                    var webEx = ExceptionFactory.CreateWebException(ex);
                    throw webEx;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {

                    serializationWatch.Stop();
                    context.Metrics.SerializationTime = serializationWatch.ElapsedMilliseconds;
                }
            }

            context.Metrics.RequestSize = client.ContentLength;
            return client;
        }

        private HttpWebRequest PrepareWebRequest(ClientExecutionContext context, NameValueCollection addHeaders = null)
        {
            if (!string.IsNullOrWhiteSpace(AppId))
            {
                if (addHeaders == null)
                    addHeaders = new NameValueCollection();
                addHeaders[ServiceUtils.AppIdHttpHeaderKey] = AppId;
            }
            string requestUri;
            if (IsSLBService)
                requestUri = context.ServiceUrl.WithTrailingSlash() + context.Operation + "." + context.Format;
            else
                requestUri = context.ServiceUrl.WithTrailingSlash() + context.Format.WithTrailingSlash() + context.Operation;

            var client = (HttpWebRequest)WebRequest.Create(requestUri);
            client.Accept = context.Accept;
            client.Method = DefaultHttpMethod;
            client.Headers.Add(Headers);

            if (!DisableConnectionLeaseTimeout)
                client.ServicePoint.ConnectionLeaseTimeout = DefaultConnectionLeaseTimeout;

            if (addHeaders != null && addHeaders.Count > 0)
            {
                client.Headers.Add(addHeaders);
            }

            if (Proxy != null) client.Proxy = Proxy;
            client.Timeout = (int)GetOperationTimeout(context.Operation, RequestTimeoutType.Timeout).TotalMilliseconds;
            client.ReadWriteTimeout = (int)GetOperationTimeout(context.Operation, RequestTimeoutType.ReadWriteTimeout).TotalMilliseconds;

            client.AutomaticDecompression = DisableAutoCompression ? DecompressionMethods.None : DefaultCompressionModes;

            client.AllowAutoRedirect = AllowAutoRedirect;

            ApplyWebRequestFilters(client);

            client.ContentType = context.ContentType;

            return client;
        }

        private TResponse HandleResponse<TResponse>(ClientExecutionContext context, WebResponse webResponse)
        {
            if (DeserializeResponseUseMemoryStream)
                return HandleResponseUseMemoryStream<TResponse>(context, webResponse);

            ApplyWebResponseFilters(webResponse);

            TResponse response;
            using (var responseStream = webResponse.GetResponseStream())
            {
                Stopwatch deserializationWatch = new Stopwatch();
                try
                {
                    deserializationWatch.Start();
                    response = (TResponse)context.CallFormat.StreamDeserializer(typeof(TResponse), responseStream);
                    context.Response = response;
                }
                catch (WebException ex)
                {
                    var webEx = ExceptionFactory.CreateWebException(ex);
                    throw webEx;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    deserializationWatch.Stop();
                    context.Metrics.DeserializationTime = deserializationWatch.ElapsedMilliseconds;
                }
            }

            if (!HandleServiceErrorManually)
                CheckResponseFailure(response as IHasResponseStatus);

            return response;
        }

        private TResponse HandleResponseUseMemoryStream<TResponse>(ClientExecutionContext context, WebResponse webResponse)
        {
            ApplyWebResponseFilters(webResponse);

            TResponse response;
            using (MemoryStream ms = new MemoryStream())
            {
                string responseTypeName = typeof(TResponse).FullName;
                using (var responseStream = webResponse.GetResponseStream())
                {
                    var readStreamTransaction = new CatTransaction("SOA2Client.deserialization.readStream", responseTypeName);
                    try
                    {
                        readStreamTransaction.Start();
                        responseStream.CopyTo(ms);
                        ms.Flush();
                        ms.Seek(0, SeekOrigin.Begin);
                        readStreamTransaction.MarkSuccess();
                    }
                    catch (Exception ex)
                    {
                        readStreamTransaction.MarkFailure(ex);
                        throw;
                    }
                    finally
                    {
                        readStreamTransaction.End();
                    }
                }

                var deserializeTransaction = new CatTransaction(CatSerializationTypes.ClientDeserializationCall, responseTypeName);
                Stopwatch deserializationWatch = new Stopwatch();
                try
                {
                    deserializationWatch.Start();
                    deserializeTransaction.Start();
                    response = (TResponse)context.CallFormat.StreamDeserializer(typeof(TResponse), ms);
                    deserializeTransaction.MarkSuccess();
                    context.Response = response;
                }
                catch (WebException ex)
                {
                    var webEx = ExceptionFactory.CreateWebException(ex);
                    deserializeTransaction.MarkFailure(webEx);
                    throw webEx;
                }
                catch (Exception ex)
                {
                    deserializeTransaction.MarkFailure(ex);
                    throw;
                }
                finally
                {
                    deserializeTransaction.End();
                    deserializationWatch.Stop();
                    context.Metrics.DeserializationTime = deserializationWatch.ElapsedMilliseconds;
                }
            }

            if (!HandleServiceErrorManually)
                CheckResponseFailure(response as IHasResponseStatus);

            return response;
        }

        private void LogGeneralException(ClientExecutionContext context, Exception ex)
        {
            string title = GetLogTitle("General Exception");
            Dictionary<string, string> addtionalInfo = GetClientInfo(context);
            addtionalInfo["ErrorCode"] = "FXD301005";
            if (!string.IsNullOrWhiteSpace(ex.HelpLink))
                addtionalInfo["HelpLink"] = ex.HelpLink;

            if (IsWebException(ex) && !LogWebExceptionAsError)
                log.Warn(title, ex, addtionalInfo);
            else
                log.Error(title, ex, addtionalInfo);
        }

        private void LogCServiceException(string title, Exception ex, Dictionary<string, string> addInfo)
        {
            if (LogCServiceExceptionAsError)
                log.Error(title, ex, addInfo);
            else
                log.Warn(title, ex, addInfo);
        }

        private bool IsWebException(Exception ex)
        {
            return ex is WebException || ex.InnerException is WebException;
        }
    }
}
