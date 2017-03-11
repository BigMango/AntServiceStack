using System;
using System.Net;
using System.Reflection;
using AntServiceStack.Common;
using AntServiceStack.Common.Utils;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.WebHost.Endpoints.Support;
using System.Collections.Generic;
using Freeway.Logging;

namespace AntServiceStack.WebHost.Endpoints
{
    /// <summary>
    /// Inherit from this class if you want to host your web services inside a 
    /// Console Application, Windows Service, etc.
    /// 
    /// Usage of HttpListener allows you to host webservices on the same port (:80) as IIS 
    /// however it requires admin user privillages.
    /// </summary>
    public abstract class AppHostHttpListenerBase
        : HttpListenerBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AppHostHttpListenerBase));

        protected AppHostHttpListenerBase() { }

        protected AppHostHttpListenerBase(params Assembly[] assembliesWithServices)
            : base(assembliesWithServices)
        {
        }

        protected AppHostHttpListenerBase(params Type[] serviceTypes)
            : base(serviceTypes)
        {
        }

        protected override void ProcessRequest(HttpListenerContext context)
        {
            if (string.IsNullOrEmpty(context.Request.RawUrl)) return;

            var operationName = context.Request.GetOperationName();

            var httpReq = new HttpListenerRequestWrapper(operationName, context.Request);
            string pathInfo;
            string servicePath;
            AntServiceStackHttpHandlerFactory.GetServicePathInfo(httpReq.PathInfo, out servicePath, out pathInfo);
            httpReq.SetServicePath(servicePath);
            var httpRes = new HttpListenerResponseWrapper(context.Response);
            HostContext.InitRequest(httpReq, httpRes);

            var handler = AntServiceStackHttpHandlerFactory.GetHandler(httpReq);
            var serviceStackHandler = handler as IServiceStackHttpHandler;
            if (serviceStackHandler != null)
            {
                var endpointHandler = serviceStackHandler as EndpointHandlerBase;
                if (endpointHandler != null) 
                {
                    httpReq.OperationName = operationName = endpointHandler.RequestName;
                    if (!string.IsNullOrWhiteSpace(operationName))
                    {
                        bool isAsync = EndpointHost.MetadataMap[endpointHandler.ServicePath.ToLower()].OperationNameMap[operationName.ToLower()].IsAsync;
                        if (isAsync)
                        {
                            var task = endpointHandler.ProcessRequestAsync(httpReq, httpRes, operationName);
                            task.ContinueWith(t =>
                            {
                                try
                                {
                                    if (t.Exception != null)
                                    {
                                        log.Error("Error happened in async service execution!", t.Exception.InnerException ?? t.Exception,
                                               new Dictionary<string, string>() { { "ErrorCode", "FXD300079" }, { "HostMode", "Self-Host" } });
                                    }
                                    httpRes.Close();
                                }
                                catch {  }
                            });
                            return;
                        }
                    }
                }

                serviceStackHandler.ProcessRequest(httpReq, httpRes, operationName);
                httpRes.Close();
                return;
            }

            throw new NotImplementedException("Cannot execute handler: " + handler + " at PathInfo: " + httpReq.PathInfo);
        }
    }
}