using System;
using System.Web;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.WebHost.Endpoints.Support;

namespace AntServiceStack.WebHost.Endpoints
{
    public class ActionHandler : IServiceStackHttpHandler, IHttpHandler
    {
        public string ServicePath { get; set; }

        public string OperationName { get; set; }

        public Func<IHttpRequest, IHttpResponse, object> Action { get; set; }

        public ActionHandler(string servicePath, Func<IHttpRequest, IHttpResponse, object> action, string operationName = null)
        {
            ServicePath = servicePath;
            Action = action;
            OperationName = operationName;
        }

        public void ProcessRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            if (Action == null)
                throw new Exception("Action was not supplied to ActionHandler");

            if (httpReq.OperationName == null)
                httpReq.SetOperationName(OperationName);

            var response = Action(httpReq, httpRes);
            httpRes.WriteToResponse(httpReq, response);
        }

        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(new Extensions.HttpRequestWrapper(ServicePath, OperationName, context.Request),
                context.Response.ToResponse(),
                OperationName);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}