using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using AntServiceStack.Common;
using AntServiceStack.Common.Extensions;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;
using AntServiceStack.WebHost.Endpoints.Extensions;
using Freeway.Logging;
using HttpRequestWrapper = AntServiceStack.WebHost.Endpoints.Extensions.HttpRequestWrapper;
using HttpResponseWrapper = AntServiceStack.WebHost.Endpoints.Extensions.HttpResponseWrapper;

namespace AntServiceStack.WebHost.Endpoints.Support
{
    public class NotFoundHttpHandler
        : IServiceStackHttpHandler, IHttpHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NotFoundHttpHandler));

        private string _servicePath;

        public bool? IsIntegratedPipeline { get; set; }
        public string WebHostPhysicalPath { get; set; }
        public string ApplicationBaseUrl { get; set; }

        public NotFoundHttpHandler(string servicePath)
        {
            _servicePath = servicePath;
        }

        public void ProcessRequest(IHttpRequest request, IHttpResponse response, string operationName)
        {
            Dictionary<string, string> logTags = new Dictionary<string, string>()
            {
                { "ErrorCode", "FXD300013" }
            };
            if (request.UrlReferrer != null && !string.IsNullOrWhiteSpace(request.UrlReferrer.AbsoluteUri))
                logTags["Referer"] = request.UrlReferrer.AbsoluteUri;
            Log.Warn(string.Format("{0} Request not found: {1}", request.RemoteIp, request.RawUrl), logTags);

            var text = new StringBuilder();

            if (EndpointHost.DebugMode)
            {
                text.AppendLine("Handler for Request not found: \n\n")
                    .AppendLine("Request.HttpMethod: " + request.HttpMethod)
                    .AppendLine("Request.ServicePath: " + _servicePath)
                    .AppendLine("Request.PathInfo: " + request.PathInfo)
                    .AppendLine("Request.QueryString: " + request.QueryString)
                    .AppendLine("Request.RawUrl: " + request.RawUrl);
            }
            else
            {
                text.Append("404");
            }

            response.ContentType = "text/plain";
            response.StatusCode = 404;
            response.EndHttpHandlerRequest(skipClose: true, afterBody: r => r.Write(text.ToString()));
        }

        public void ProcessRequest(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            var httpReq = new HttpRequestWrapper(_servicePath, "NotFoundHttpHandler", request);
            var httpRes = new HttpResponseWrapper(response);
            HostContext.InitRequest(httpReq, httpRes);
            if (!request.IsLocal)
            {
                ProcessRequest(httpReq, new HttpResponseWrapper(response), null);
                return;
            }

            Dictionary<string, string> logTags = new Dictionary<string, string>()
            {
                { "ErrorCode", "FXD300013" }
            };
            if (httpReq.UrlReferrer != null && !string.IsNullOrWhiteSpace(httpReq.UrlReferrer.AbsoluteUri))
                logTags["Referer"] = httpReq.UrlReferrer.AbsoluteUri;
            Log.Warn(string.Format("{0} Request not found: {1}", httpReq.RemoteIp, httpReq.RawUrl), logTags);

            var sb = new StringBuilder();
            sb.AppendLine("Handler for Request not found: \n\n");

            sb.AppendLine("Request.ApplicationPath: " + request.ApplicationPath);
            sb.AppendLine("Request.CurrentExecutionFilePath: " + request.CurrentExecutionFilePath);
            sb.AppendLine("Request.FilePath: " + request.FilePath);
            sb.AppendLine("Request.HttpMethod: " + request.HttpMethod);
            sb.AppendLine("Request.MapPath('~'): " + request.MapPath("~"));
            sb.AppendLine("Request.Path: " + request.Path);
            sb.AppendLine("Request.ServicePath: " + _servicePath);
            sb.AppendLine("Request.PathInfo: " + request.PathInfo);
            sb.AppendLine("Request.ResolvedPathInfo: " + httpReq.PathInfo);
            sb.AppendLine("Request.PhysicalPath: " + request.PhysicalPath);
            sb.AppendLine("Request.PhysicalApplicationPath: " + request.PhysicalApplicationPath);
            sb.AppendLine("Request.QueryString: " + request.QueryString);
            sb.AppendLine("Request.RawUrl: " + request.RawUrl);
            sb.AppendLine("Request.Referer: " + (request.UrlReferrer == null ? string.Empty : request.UrlReferrer.AbsoluteUri));
            try
            {
                sb.AppendLine("Request.Url.AbsoluteUri: " + request.Url.AbsoluteUri);
                sb.AppendLine("Request.Url.AbsolutePath: " + request.Url.AbsolutePath);
                sb.AppendLine("Request.Url.Fragment: " + request.Url.Fragment);
                sb.AppendLine("Request.Url.Host: " + request.Url.Host);
                sb.AppendLine("Request.Url.LocalPath: " + request.Url.LocalPath);
                sb.AppendLine("Request.Url.Port: " + request.Url.Port);
                sb.AppendLine("Request.Url.Query: " + request.Url.Query);
                sb.AppendLine("Request.Url.Scheme: " + request.Url.Scheme);
                sb.AppendLine("Request.Url.Segments: " + request.Url.Segments);
            }
            catch (Exception ex)
            {
                sb.AppendLine("Request.Url ERROR: " + ex.Message);
            }
            if (IsIntegratedPipeline.HasValue)
                sb.AppendLine("App.IsIntegratedPipeline: " + IsIntegratedPipeline);
            if (!WebHostPhysicalPath.IsNullOrEmpty())
                sb.AppendLine("App.WebHostPhysicalPath: " + WebHostPhysicalPath);
            if (!ApplicationBaseUrl.IsNullOrEmpty())
                sb.AppendLine("App.ApplicationBaseUrl: " + ApplicationBaseUrl);
            if (!AntServiceStackHttpHandlerFactory.DebugLastHandlerArgs.IsNullOrEmpty())
                sb.AppendLine("App.DebugLastHandlerArgs: " + AntServiceStackHttpHandlerFactory.DebugLastHandlerArgs);

            response.ContentType = "text/plain";
            response.StatusCode = 404;
            response.EndHttpHandlerRequest(skipClose: true, afterBody: r => r.Write(sb.ToString()));
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}