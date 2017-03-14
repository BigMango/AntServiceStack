using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using AntServiceStack.Text;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Execution;
using AntServiceStack.Client.CHystrix;

namespace AntServiceStack.ServiceClient
{
    public abstract partial class ServiceClientBase<DerivedClient> : ServiceClientBase
        where DerivedClient : ServiceClientBase<DerivedClient> 
    {
        /// <summary>
        /// 启动异步请求Task，IOCP实现
        /// </summary>
        /// <typeparam name="TResponse">服务响应</typeparam>
        /// <param name="operationName">目标服务操作名</param>
        /// <param name="requestObject">服务请求对象</param>
        /// <returns>异步请求Task</returns>
        public Task<TResponse> StartIOCPTask<TResponse>(string operationName, object requestObject)
        {
            AsyncRequestState asyncRequestState = new AsyncRequestState();
            TaskCompletionSource<TResponse> taskCompletionSource = new TaskCompletionSource<TResponse>(asyncRequestState);
            Stopwatch stopwatch = new Stopwatch();
            Stopwatch requestWatch = new Stopwatch();
            Stopwatch responseWatch = new Stopwatch();
            ClientExecutionContext context = CreateExecutionContext(operationName, requestObject, ExecutionModes.IOCPMode);

            
            object semaphoreIsolationInstance = CreateCHystrixSemaphoreIsolationInstance(context);

            #region General Error Handler
            
            Action<Exception> handleGeneralError = ex =>
            {
                try
                {
                    if (asyncRequestState != null)
                        asyncRequestState.Complete();

                    stopwatch.Stop();
                    context.Metrics.TotalTime = stopwatch.ElapsedMilliseconds;
                }
                catch { }

                try
                {
                    SemaphoreIsolationMarkFailure(context, semaphoreIsolationInstance);
                }
                catch { }

                try
                {
                    if (ex is WebException)
                        ex = HandleWebException(context, (WebException)ex);


                    if (semaphoreIsolationInstance == null || ex.GetType().FullName != CHystrixIntegration.CHystrixExceptionName)
                        LogGeneralException(context, ex);

                    context.IsSuccess = false;
                    context.Error = ex;
                }
                catch { }

                taskCompletionSource.TrySetException(ex);
            };

            #endregion

            try
            {

                SemaphoreIsolationStartExecution(semaphoreIsolationInstance);

                stopwatch.Start();

                Validate(context);
                ApplyRequestFilters(ServiceName, ServiceNamespace, operationName, requestObject);

                HttpWebRequest webRequest = PrepareWebRequest(context,null);
                asyncRequestState.Initialize(webRequest, _enableTimeoutForIOCPAsync);
                
                #region Response CallBack

                AsyncCallback getResponseCallback = asyncGetResponseResult =>
                {
                    try
                    {
                        if (asyncGetResponseResult.CompletedSynchronously)
                        {
                        }
                        else
                        {
                            InitThreadLocalBaseUri(context);
                        }

                        using (WebResponse webResponse = EndGetResponse(context, webRequest, asyncGetResponseResult))
                        {
                            responseWatch.Stop();
                            context.Metrics.ResponseSize = webResponse.ContentLength < 0 ? 0 : webResponse.ContentLength;

                            TResponse response = HandleResponse<TResponse>(context, webResponse);
                            asyncRequestState.Complete();
                            ApplyResponseFilters(ServiceName, ServiceNamespace, operationName, response);

                            taskCompletionSource.SetResult(response);

                            try
                            {
                                context.IsSuccess = true;

                                context.Metrics.TotalTime = stopwatch.ElapsedMilliseconds;
                            }
                            catch { }

                            try
                            {
                                SemaphoreIsolationMarkSuccess(context, semaphoreIsolationInstance);
                            }
                            catch { }
                        }

                    }
                    catch (CServiceException cex)
                    {
                        try
                        {
                            asyncRequestState.Complete();

                            Dictionary<string, string> addtionalInfo = GetClientInfo(context);
                            addtionalInfo["ErrorCode"] = "FXD301004";
                            if (LogErrorWithRequestInfo)
                                addtionalInfo["RequestObject"] = TypeSerializer.SerializeToString(requestObject);
                            LogCServiceException(GetLogTitle(InternalServiceUtils.ServiceErrorTitle), cex, addtionalInfo);


                            stopwatch.Stop();

                            context.IsSuccess = false;
                            context.Error = cex;
                        }
                        catch { }

                        try
                        {
                            if (IsCServiceExceptionValidationError(cex))
                                SemaphoreIsolationMarkBadRequest(context, semaphoreIsolationInstance);
                            else
                                SemaphoreIsolationMarkFailure(context, semaphoreIsolationInstance);
                        }
                        catch { }

                        taskCompletionSource.TrySetException(cex);
                    }
                    catch (Exception ex1)
                    {
                        handleGeneralError(ex1);
                    }
                    finally
                    {
                        try
                        {
                            if (responseWatch.IsRunning)
                                responseWatch.Stop();
                            context.Metrics.ExecutionTime = responseWatch.ElapsedMilliseconds;
                        }
                        catch { }

                        ApplyRequestEndFilterSafe(context);

                    }
                };

                #endregion

                #region Request CallBack

                AsyncCallback getRequestCallback = asyncGetRequestResult =>
                {
                    bool success = true;
                    try
                    {
                        if (asyncGetRequestResult.CompletedSynchronously)
                        {
                        }
                        else
                        {
                            InitThreadLocalBaseUri(context);
                        }
                        using (Stream requestStream = EndGetRequestStream(context, webRequest, asyncGetRequestResult))
                        {
                            try
                            {
                                context.CallFormat.StreamSerializer(requestObject, requestStream);
                            }
                            catch (Exception )
                            {
                                throw;
                            }
                            finally
                            {
                            }
                        }

                        context.Metrics.RequestSize = webRequest.ContentLength;

                        responseWatch.Start();
                        BeginGetResponse(context, webRequest, getResponseCallback);
                    }
                    catch (Exception ex2)
                    {
                        success = false;
                        handleGeneralError(ex2);
                    }
                    finally
                    {
                        try
                        {
                            requestWatch.Stop();
                            context.Metrics.SerializationTime = requestWatch.ElapsedMilliseconds;
                        }
                        catch { }

                        if (!success)
                            ApplyRequestEndFilterSafe(context);

                    }
                };

                #endregion

                requestWatch.Start();
                BeginGetRequestStream(context, webRequest, getRequestCallback);
            }
            catch (Exception ex3)
            {
                handleGeneralError(ex3);
                ApplyRequestEndFilterSafe(context);
            }
            finally
            {
                ResetCurrentRequestTimeoutSetting();

            }

            return TryAddFaultHandler(taskCompletionSource.Task);
        }

        private void BeginGetRequestStream(ClientExecutionContext context, HttpWebRequest httpRequest, AsyncCallback requestStreamCallback)
        {
            try
            {
                httpRequest.BeginGetRequestStream(requestStreamCallback, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Stream EndGetRequestStream(ClientExecutionContext context, HttpWebRequest httpRequest, IAsyncResult asyncResult)
        {
            try
            {
                return httpRequest.EndGetRequestStream(asyncResult);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void BeginGetResponse(ClientExecutionContext context, HttpWebRequest httpRequest, AsyncCallback responseCallback)
        {
            try
            {
                httpRequest.BeginGetResponse(responseCallback, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private WebResponse EndGetResponse(ClientExecutionContext context, HttpWebRequest httpRequest, IAsyncResult asyncResult)
        {
            try
            {
                WebResponse response = httpRequest.EndGetResponse(asyncResult);
                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private object CreateCHystrixSemaphoreIsolationInstance(ClientExecutionContext context)
        {
            object chystrixSemaphoreIsolationInstance = null;
            try
            {
                if (EnableCHystrixSupportForIOCPAsync)
                {
                    string chystrixCommandKey;
                    string chystrixInstanceKey;
                    GetCHystrixCommandKeyForIOCPAsync(context, out chystrixCommandKey, out chystrixInstanceKey);
                    chystrixSemaphoreIsolationInstance = CHystrixIntegration.UtilsSemaphoreIsolationCreateInstance(chystrixInstanceKey, chystrixCommandKey);
                }
            }
            catch (Exception ex)
            {
                Dictionary<string, string> addtionalInfo = GetClientInfo(context);
                addtionalInfo["ErrorCode"] = "FXD301014";
                log.Warn("CHystrix IOCP Support CreateInstance failed", ex, addtionalInfo);
            }

            return chystrixSemaphoreIsolationInstance;
        }

        private void SemaphoreIsolationStartExecution(object instance)
        {
            if (instance == null)
                return;

            CHystrixIntegration.UtilsSemaphoreIsolationStartExecution(instance);
        }

        private void SemaphoreIsolationMarkSuccess(ClientExecutionContext context, object instance)
        {
            try
            {
                if (instance == null)
                    return;

                CHystrixIntegration.UtilsSemaphoreIsolationMarkSuccess(instance);
            }
            catch (Exception ex)
            {
                Dictionary<string, string> addtionalInfo = GetClientInfo(context);
                addtionalInfo["ErrorCode"] = "FXD301015";
                log.Warn("CHystrix IOCP Support MarkSuccess failed", ex, addtionalInfo);
            }

            SemaphoreIsolationEndExecution(context, instance);
        }

        private void SemaphoreIsolationMarkFailure(ClientExecutionContext context, object instance)
        {
            try
            {
                if (instance == null)
                    return;

                CHystrixIntegration.UtilsSemaphoreIsolationMarkFailure(instance);
            }
            catch (Exception ex)
            {
                Dictionary<string, string> addtionalInfo = GetClientInfo(context);
                addtionalInfo["ErrorCode"] = "FXD301016";
                log.Warn("CHystrix IOCP Support MarkFailure failed", ex, addtionalInfo);
            }

            SemaphoreIsolationEndExecution(context, instance);
        }

        private void SemaphoreIsolationMarkBadRequest(ClientExecutionContext context, object instance)
        {
            try
            {
                if (instance == null)
                    return;

                CHystrixIntegration.UtilsSemaphoreIsolationMarkBadRequest(instance);
            }
            catch (Exception ex)
            {
                Dictionary<string, string> addtionalInfo = GetClientInfo(context);
                addtionalInfo["ErrorCode"] = "FXD301017";
                log.Warn("CHystrix IOCP Support MarkBadRequest failed", ex, addtionalInfo);
            }

            SemaphoreIsolationEndExecution(context, instance);
        }

        private void SemaphoreIsolationEndExecution(ClientExecutionContext context, object instance)
        {
            try
            {
                if (instance == null)
                    return;

                CHystrixIntegration.UtilsSemaphoreIsolationEndExecution(instance);
            }
            catch (Exception ex)
            {
                Dictionary<string, string> addtionalInfo = GetClientInfo(context);
                addtionalInfo["ErrorCode"] = "FXD301018";
                log.Warn("CHystrix IOCP Support EndExecution failed", ex, addtionalInfo);
            }
        }
    }
}
