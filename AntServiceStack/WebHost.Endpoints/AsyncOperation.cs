using System;
using System.Web;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using AntServiceStack.WebHost.Endpoints.Support;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.Threading;
using Freeway.Logging;
using AntServiceStack.ServiceHost;
using AntServiceStack.Common;
using AntServiceStack.Common.Types;
using AntServiceStack.Common.Hystrix;
using AntServiceStack.Common.Utils;
using HttpRequestExtensions = AntServiceStack.WebHost.Endpoints.Extensions.HttpRequestExtensions;
using HttpRequestWrapper = AntServiceStack.WebHost.Endpoints.Extensions.HttpRequestWrapper;
using HttpResponseWrapper = AntServiceStack.WebHost.Endpoints.Extensions.HttpResponseWrapper;

using AntServiceStack.Text;
using System.Threading.Tasks;
using AntServiceStack.WebHost.Endpoints.Utils;
using AntServiceStack.Common.Web;
using AntServiceStack.Common.CAT;

namespace AntServiceStack.WebHost.Endpoints
{
    public class AsyncOperation : IAsyncResult
    {
        internal static readonly ILog Log = LogManager.GetLogger(typeof(AsyncOperation));

        private bool _completed;
        private Object _state;
        private AsyncCallback _callback;
        private HttpContext _context;
        private EndpointHandlerBase _endpointHandler;
        private DateTime _startTime;
        private volatile IExecutionResult _executionResult;
        private bool _executedSynchronously = false;
        private bool _isAsync;
        private bool _slaErrorSent;

        private IHttpRequest _httpRequest;
        private IHttpResponse _httpResponse;
        private Operation _operation;

        bool IAsyncResult.IsCompleted 
        { 
            get 
            {
                return _completed; 
            } 
        }
        
        WaitHandle IAsyncResult.AsyncWaitHandle 
        { 
            get 
            { 
                return null; 
            } 
        }
        
        Object IAsyncResult.AsyncState { get { return _state; } }

        bool IAsyncResult.CompletedSynchronously
        {
            get
            {
                return _executedSynchronously;
            }
        }
        
        public bool ExecutedSynchronously { get { return _executedSynchronously; } }
        public DateTime StartTime { get { return _startTime; } }
        public IExecutionResult ExecutionResult { get { return _executionResult; } }
        public bool IsAsync { get { return _isAsync; } }
        public bool SLAErrorSent { get { return _slaErrorSent; } }

        public string ServicePath { get; private set; }
        public string OperationName { get; private set; }


        public AsyncOperation(AsyncCallback callback, HttpContext context, Object state, EndpointHandlerBase endpointHandler, string servicePath, string operationName)
        {
            _callback = callback;
            _context = context;
            _state = state;
            _completed = false;
            _endpointHandler = endpointHandler;
            ServicePath = servicePath;
            OperationName = operationName.ToLower();
            _operation = EndpointHost.MetadataMap[ServicePath].OperationNameMap[OperationName];
            _isAsync = _operation.IsAsync;

            Initialize();
        }

        public void SendSyncSLAErrorResponse(HystrixEventType hystrixEvent)
        {
            _executedSynchronously = true; // indicating this is executed synchronously
            _slaErrorSent = true;
            _startTime = DateTime.Now;

            _httpRequest.InputStream.Close(); // do not care

            var message = "服务端发生了熔断，并进入了自我保护状态，服务端恢复后此错误会自动消失！";
            _executionResult.ExceptionCaught = new Exception(message);

            if (this._endpointHandler != null)
            {
                _endpointHandler.SendSyncSLAErrorResponse(_httpRequest, _httpResponse, hystrixEvent);
            }
        }

        public void StartAsyncWork()
        {
            _executedSynchronously = false;
            _startTime = DateTime.Now;

            try
            {
                Task task = _endpointHandler.ProcessRequestAsync(_httpRequest, _httpResponse, OperationName);
                task.ContinueWith(t =>
                {
                    if (t.Exception != null)
                        Log.Error("Error happened in async service execution!", t.Exception.InnerException ?? t.Exception,
                            new Dictionary<string, string>() { { "ErrorCode", "FXD300079" } });

                    PostWorkItemExecutionCallbackSafe();
                });
            }
            catch (Exception ex)
            {
                try
                {
                    var operation = EndpointHost.MetadataMap[ServicePath].OperationNameMap[OperationName];
                    var response = ErrorUtils.CreateFrameworkErrorResponse(_httpRequest, ex, operation.ResponseType);
                    _httpResponse.WriteToResponse(_httpRequest, response);
                }
                catch (Exception ex1)
                {
                    Log.Error("Error happened when writing framework error response!", ex1, new Dictionary<string, string>() { { "ErrorCode", "FXD300082" } });
                }

                PostWorkItemExecutionCallbackSafe();
            }
        }

        // used in sync mode
        public void StartSyncWork()
        {
            _executedSynchronously = true; // indicating this is executed synchronously
            _startTime = DateTime.Now;
            
            try
            {
                _endpointHandler.ProcessRequest(_httpRequest, _httpResponse, OperationName);
            }
            finally
            {
                LogMessage(_httpRequest, _httpResponse);
                PostWorkItemExecutionCallback();
            }
        }

        private void Initialize()
        {
            _httpRequest = new HttpRequestWrapper(ServicePath, OperationName, _context.Request);
            _httpResponse = new HttpResponseWrapper(_context.Response);
            HostContext.InitRequest(_httpRequest, _httpResponse);
            _executionResult = _httpResponse.ExecutionResult;
            _httpRequest.Items[InternalServiceUtils.SOA2CurrentOperationKey] = _operation.Key;
        }

        internal static void LogMessage(IHttpRequest httpRequest, IHttpResponse httpResponse)
        {
            try
            {
                ServiceMetadata serviceMetadata = EndpointHost.Config.MetadataMap[httpRequest.ServicePath];

                Dictionary<string, string> requestData = new Dictionary<string, string>();
                if (httpRequest.RequestObject != null
                    && httpRequest.ContentLength > 0
                    && httpRequest.ContentLength <= MessageLogConfig.RequestLogMaxSize
                    && serviceMetadata.CanLogRequest(httpRequest.OperationName))
                {
                    requestData.Add("request", JsonSerializer.SerializeToString(httpRequest.RequestObject));
                }


                Dictionary<string, string> responseData = new Dictionary<string, string>();
                if (httpResponse.ResponseObject != null
                    && httpResponse.ExecutionResult != null
                    && httpResponse.ExecutionResult.ResponseSize > 0
                    && httpResponse.ExecutionResult.ResponseSize <= MessageLogConfig.ResponseLogMaxSize
                    && serviceMetadata.CanLogResponse(httpRequest.OperationName))
                {
                    responseData.Add("response", JsonSerializer.SerializeToString(httpResponse.ResponseObject));
                }

            }
            catch (Exception ex)
            {
                Log.Warn("Failed to log request/response messages!", ex, new Dictionary<string, string>() { { "ErrorCode", "FXD300032" } });
            }
        }
        
        private void PostWorkItemExecutionCallback()
        {
            if (!_completed)
            {
                _completed = true;
                _callback(this);
            }
        }

        private void PostWorkItemExecutionCallbackSafe()
        {
            try
            {
                PostWorkItemExecutionCallback();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to execute post workitem execution callback!", ex, new Dictionary<string, string>() { { "ErrorCode", "FXD300083" } });
            }
        }
        
        internal void ContinueCatTransaction()
        {
            string identity = EndpointHost.Config.MetadataMap[ServicePath].OperationNameMap[OperationName].Key;
            
            if (_httpRequest.IsH5GatewayRequest())
                return;

            string clientAppId = _httpRequest.GetClientAppId();
            if (string.IsNullOrWhiteSpace(clientAppId))
                return;

            if (!string.IsNullOrWhiteSpace(ServiceUtils.AppId))
            {
                _context.Response.AddHeader(ServiceUtils.ServiceAppIdHttpHeaderKey, ServiceUtils.AppId);
                _context.Response.AddHeader(ServiceUtils.ESBServiceAppIdHttpHeaderKey, ServiceUtils.AppId);
            }

            _context.Response.AddHeader(ServiceUtils.ServiceHostIPHttpHeaderKey, ServiceUtils.HostIP);
            _context.Response.AddHeader(ServiceUtils.ESBServiceHostIPHttpHeaderKey, ServiceUtils.HostIP);
        }

        internal void CompleteCatTransaction()
        {
           
        }

      
    }
}
