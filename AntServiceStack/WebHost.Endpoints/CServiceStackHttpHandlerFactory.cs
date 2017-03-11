using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web;
using AntServiceStack.Common;
using AntServiceStack.Common.Extensions;
using AntServiceStack.Common.Utils;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.WebHost.Endpoints.Support;
using HttpRequestWrapper = AntServiceStack.WebHost.Endpoints.Extensions.HttpRequestWrapper;

namespace AntServiceStack.WebHost.Endpoints
{
    public class AntServiceStackHttpHandlerFactory
        : IHttpHandlerFactory
    {
        private static readonly string WebHostPhysicalPath = null;
        private static readonly Dictionary<string, string> RedirectPaths;
        private static readonly bool IsIntegratedPipeline = false;
        private static Func<IHttpRequest, IHttpHandler>[] RawHttpHandlers;

        [ThreadStatic]
        public static string DebugLastHandlerArgs;

        static AntServiceStackHttpHandlerFactory()
        {
            if (EndpointHost.Config.ServiceManager == null)
            {
                throw new ConfigurationErrorsException(
                    "AntServiceStack: AppHost does not exist or has not been initialized. "
                    + "Make sure you have created an AppHost and started it with 'new AppHost().Init();' in your Global.asax Application_Start()");
            }

            //MONO doesn't implement this property
            var pi = typeof(HttpRuntime).GetProperty("UsingIntegratedPipeline");
            if (pi != null)
            {
                IsIntegratedPipeline = (bool)pi.GetGetMethod().Invoke(null, new object[0]);
            }

            WebHostPhysicalPath = EndpointHost.Config.WebHostPhysicalPath;

            RedirectPaths = new Dictionary<string, string>();
            foreach (string servicePath in EndpointHost.Config.ServicePaths)
            {
                string redirectRelativeurl = EndpointHost.Config.MetadataRedirectPath;
                if (servicePath != ServiceMetadata.EmptyServicePath)
                    redirectRelativeurl = PathUtils.CombinePaths(EndpointHost.Config.ServiceStackHandlerFactoryPath, servicePath, "metadata");
                RedirectPaths[servicePath] = redirectRelativeurl;
            }

            var rawHandlers = EndpointHost.Config.RawHttpHandlers;
            rawHandlers.Add(ReturnRequestInfo);
            rawHandlers.Add(ReturnHystrixGlobalStreamHandler);
            RawHttpHandlers = rawHandlers.ToArray();
        }

        private static IHttpHandler GetNotFoundHttpHandler(string servicePath)
        {
            IHttpHandler handler = EndpointHost.Config.GetHandlerForErrorStatus(HttpStatusCode.NotFound, servicePath);
            if (handler == null)
            {
                handler = new NotFoundHttpHandler(servicePath)
                {
                    IsIntegratedPipeline = IsIntegratedPipeline,
                    WebHostPhysicalPath = WebHostPhysicalPath,
                };
            }

            return handler;
        }

        internal static IHttpHandler GetRedirectHandler(string applicationUrl, string servicePath)
        {
            return new RedirectHttpHandler(servicePath)
            {
                RelativeUrl = RedirectPaths[servicePath],
                AbsoluteUrl = applicationUrl.CombineWith(RedirectPaths[servicePath])
            };
        }

        public static void GetServicePathInfo(string pathInfo, out string servicePath, out string servicePathInfo)
        {
            servicePath = ServiceMetadata.EmptyServicePath;
            servicePathInfo = pathInfo;
            if (!string.IsNullOrWhiteSpace(pathInfo))
            {
                string[] parts = pathInfo.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    string firstPath = parts[0].ToLower().Trim();
                    if (EndpointHost.Config.ServicePaths.Contains(firstPath))
                    {
                        servicePath = firstPath;
                        servicePathInfo = pathInfo.Substring(parts[0].Length + (pathInfo.StartsWith("/") ? 1 : 0));
                    }
                }
            }
        }

        public static bool IsValidServicePath(string servicePath)
        {
            return EndpointHost.Config.ServicePaths.Contains(servicePath);
        }

        // Entry point for ASP.NET
        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            DebugLastHandlerArgs = requestType + "|" + url + "|" + pathTranslated;

            var pathInfo = context.Request.GetPathInfo();
            string servicePath;
            GetServicePathInfo(pathInfo, out servicePath, out pathInfo);

            var httpReq = new HttpRequestWrapper(servicePath, pathTranslated, context.Request);
            foreach (var rawHttpHandler in RawHttpHandlers)
            {
                var reqInfo = rawHttpHandler(httpReq);
                if (reqInfo != null) return reqInfo;
            }

            var mode = EndpointHost.Config.ServiceStackHandlerFactoryPath;
            if (mode == null && (url == "/default.htm" || url == "Default.htm" || url == "/default.aspx" || url == "/Default.aspx"))
                pathInfo = "/";

            // Redirect to metadata page
            if (string.IsNullOrEmpty(pathInfo) || pathInfo == "/")
            {
                if (EndpointHost.Config.HostMultipleServices && servicePath == ServiceMetadata.EmptyServicePath)
                    return new MultiServiceMetadataHandler();

                if (IsValidServicePath(servicePath))
                    return GetRedirectHandler(context.Request.GetApplicationUrl(), servicePath);
            }

            return GetHandlerForPathInfo(
                context.Request.HttpMethod, servicePath, pathInfo, context.Request.FilePath, pathTranslated)
                   ?? GetNotFoundHttpHandler(servicePath);
        }

        // Entry point for HttpListener
        public static IHttpHandler GetHandler(IHttpRequest httpReq)
        {
            foreach (var rawHttpHandler in RawHttpHandlers)
            {
                var reqInfo = rawHttpHandler(httpReq);
                if (reqInfo != null) return reqInfo;
            }

            // Redirect to metadata page
            if (string.IsNullOrEmpty(httpReq.PathInfo) || httpReq.PathInfo == "/")
            {
                if (EndpointHost.Config.HostMultipleServices && httpReq.ServicePath == ServiceMetadata.EmptyServicePath)
                    return new MultiServiceMetadataHandler();

                if (IsValidServicePath(httpReq.ServicePath))
                    return GetRedirectHandler(httpReq.GetPathUrl(), httpReq.ServicePath);
            }

            return GetHandlerForPathInfo(httpReq.HttpMethod, httpReq.ServicePath, httpReq.PathInfo, httpReq.PathInfo, httpReq.GetPhysicalPath())
                   ?? GetNotFoundHttpHandler(httpReq.ServicePath);
        }

        private static IHttpHandler ReturnRequestInfo(IHttpRequest httpReq)
        {
            if (EndpointHost.Config.DebugOnlyReturnRequestInfo
                || (EndpointHost.DebugMode && httpReq.PathInfo.EndsWith("__requestinfo")))
            {
                var reqInfo = RequestInfoHandler.GetRequestInfo(httpReq);

                reqInfo.Host = EndpointHost.Config.DebugHttpListenerHostEnvironment + "_v" + Env.AntServiceStackVersion + "_" + EndpointHost.Config.MetadataMap[httpReq.ServicePath].FullServiceName;
                reqInfo.PathInfo = httpReq.PathInfo;
                reqInfo.Path = httpReq.GetPathUrl();

                return new RequestInfoHandler(httpReq.ServicePath) { RequestInfo = reqInfo };
            }

            return null;
        }

        private static IHttpHandler ReturnHystrixGlobalStreamHandler(IHttpRequest httpReq)
        {
            if (httpReq.ServicePath == ServiceMetadata.EmptyServicePath && !string.IsNullOrWhiteSpace(httpReq.PathInfo))
            {
                string pathInfo = httpReq.PathInfo.TrimStart('/').ToLower();
                if (pathInfo == HystrixGlobalStreamHandler.RestPath || pathInfo == HystrixGlobalStreamHandler.NonStreamRestPath)
                    return new HystrixGlobalStreamHandler(httpReq.ServicePath, pathInfo == HystrixGlobalStreamHandler.NonStreamRestPath);
            }

            return null;
        }

        public static IHttpHandler GetHandlerForPathInfo(string httpMethod, string servicePath, string pathInfo, string requestPath, string filePath)
        {
            if (!IsValidServicePath(servicePath))
                return GetNotFoundHttpHandler(servicePath);

            var pathParts = pathInfo.TrimStart('/').Split('/');
            if (pathParts.Length == 0) return GetNotFoundHttpHandler(servicePath);

            // try generic handler first
            var handler = GetCatchAllHandlerIfAny(httpMethod, servicePath, pathInfo, filePath);
            if (handler != null) return handler;

            // try restful handler
            string contentType;
            var restPath = RestHandler.FindMatchingRestPath(httpMethod, servicePath, pathInfo, out contentType);
            if (restPath != null)
                return new RestHandler(servicePath) { RestPath = restPath, RequestName = restPath.OperationName, ResponseContentType = contentType };

            // try fallback handler
            FallbackRestPathDelegate fallback;
            EndpointHost.Config.FallbackRestPaths.TryGetValue(servicePath, out fallback);
            if (fallback == null)
                fallback = EndpointHost.Config.FallbackRestPath;
            if (fallback != null)
            {
                restPath = fallback(httpMethod, servicePath, pathInfo, filePath);
                if (restPath != null)
                {
                    return new RestHandler(servicePath) { RestPath = restPath, RequestName = restPath.OperationName, ResponseContentType = contentType };
                }
            }

            return null;
        }

        private static IHttpHandler GetCatchAllHandlerIfAny(string httpMethod, string servicePath, string pathInfo, string filePath)
        {
            if (EndpointHost.CatchAllHandlers != null)
            {
                foreach (var httpHandlerResolver in EndpointHost.CatchAllHandlers)
                {
                    var httpHandler = httpHandlerResolver(httpMethod, servicePath, pathInfo, filePath);
                    if (httpHandler != null)
                        return httpHandler;
                }
            }

            return null;
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
        }
    }
}