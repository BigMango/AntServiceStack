using System.Collections.Generic;
using System.Web;
using AntServiceStack.Common;
using AntServiceStack.Common.Extensions;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;
using AntServiceStack.WebHost.Endpoints.Extensions;
using EndpointsExtensions = AntServiceStack.WebHost.Endpoints.Extensions;

namespace AntServiceStack.WebHost.Endpoints.Support
{
    public class ForbiddenHttpHandler
        : IServiceStackHttpHandler, IHttpHandler
    {
        private string _servicePath;

        public bool? IsIntegratedPipeline { get; set; }
        public string WebHostPhysicalPath { get; set; }
        public string ApplicationBaseUrl { get; set; }

        public ForbiddenHttpHandler(string servicePath)
        {
            _servicePath = servicePath;
        }

        public void ProcessRequest(IHttpRequest request, IHttpResponse response, string operationName)
        {
            response.ContentType = "text/plain";
            response.StatusCode = 403;

            response.LogRequest(request);
            response.EndHttpHandlerRequest(skipClose: true, afterBody: r =>
            {
                r.Write("Forbidden\n\n");

                r.Write("\nRequest.HttpMethod: " + request.HttpMethod);
                r.Write("\nRequest.ServicePath: " + _servicePath);
                r.Write("\nRequest.PathInfo: " + request.PathInfo);
                r.Write("\nRequest.QueryString: " + request.QueryString);
                r.Write("\nRequest.RawUrl: " + request.RawUrl);

                if (IsIntegratedPipeline.HasValue)
                    r.Write("\nApp.IsIntegratedPipeline: " + IsIntegratedPipeline);
                if (!WebHostPhysicalPath.IsNullOrEmpty())
                    r.Write("\nApp.WebHostPhysicalPath: " + WebHostPhysicalPath);
                if (!ApplicationBaseUrl.IsNullOrEmpty())
                    r.Write("\nApp.ApplicationBaseUrl: " + ApplicationBaseUrl);
                if (!AntServiceStackHttpHandlerFactory.DebugLastHandlerArgs.IsNullOrEmpty())
                    r.Write("\nApp.DebugLastHandlerArgs: " + AntServiceStackHttpHandlerFactory.DebugLastHandlerArgs);
            });
        }

        public void ProcessRequest(HttpContext context)
        {
            IHttpRequest request = new EndpointsExtensions.HttpRequestWrapper(_servicePath, typeof(ForbiddenHttpHandler).Name, context.Request);
            IHttpResponse response = new EndpointsExtensions.HttpResponseWrapper(context.Response);
            HostContext.InitRequest(request, response);
            ProcessRequest(request, response, typeof(ForbiddenHttpHandler).Name);
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}