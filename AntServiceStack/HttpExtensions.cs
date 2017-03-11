using System;
using System.Net;
using System.Web;
using System.Collections.Generic;
using System.Configuration;
using AntServiceStack.Text;
using AntServiceStack.Common;
using AntServiceStack.Common.Types;
using AntServiceStack.Common.Web;
using AntServiceStack.Common.Utils;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.WebHost.Endpoints.Support;

namespace AntServiceStack
{
    public static class HttpExtensions
    {

        public static HttpRequestContext ToRequestContext(this HttpContext httpContext, object requestDto = null)
        {
            return new HttpRequestContext(
                httpContext.Request.ToRequest(),
                httpContext.Response.ToResponse(),
                requestDto);
        }

        public static HttpRequestContext ToRequestContext(this HttpListenerContext httpContext, object requestDto = null)
        {
            return new HttpRequestContext(
                httpContext.Request.ToRequest(),
                httpContext.Response.ToResponse(),
                requestDto);
        }

        /// <summary>
        /// End a ServiceStack Request
        /// </summary>
        public static void EndRequest(this HttpResponse httpRes, bool skipHeaders = false)
        {
            if (!skipHeaders) httpRes.ApplyGlobalResponseHeaders();
            httpRes.Close();
            EndpointHost.CompleteRequest();
        }

        /// <summary>
        /// End a ServiceStack Request
        /// </summary>
        public static void EndRequest(this IHttpResponse httpRes, bool skipHeaders = false)
        {
            httpRes.EndHttpHandlerRequest(skipHeaders: skipHeaders);
            EndpointHost.CompleteRequest();
        }

        /// <summary>
        /// End a HttpHandler Request
        /// </summary>
        public static void EndHttpHandlerRequest(this HttpResponse httpRes, bool skipHeaders = false, bool skipClose = false, bool closeOutputStream = false, Action<HttpResponse> afterBody = null)
        {
            if (!skipHeaders) httpRes.ApplyGlobalResponseHeaders();
            if (afterBody != null) afterBody(httpRes);
            if (closeOutputStream) httpRes.CloseOutputStream();
            else if (!skipClose) httpRes.Close();

            //skipHeaders used when Apache+mod_mono doesn't like:
            //response.OutputStream.Flush();
            //response.Close();
        }

        /// <summary>
        /// End a HttpHandler Request
        /// </summary>
        public static void EndHttpHandlerRequest(this IHttpResponse httpRes, bool skipHeaders = false, bool skipClose = false, Action<IHttpResponse> afterBody = null)
        {
            if (!skipHeaders) httpRes.ApplyGlobalResponseHeaders();
            if (afterBody != null) afterBody(httpRes);
            if (!skipClose) httpRes.Close();

            //skipHeaders used when Apache+mod_mono doesn't like:
            //response.OutputStream.Flush();
            //response.Close();
        }
       
        /// <summary>
        /// End a ServiceStack Request with no content
        /// </summary>
        public static void EndRequestWithNoContent(this IHttpResponse httpRes)
        {
            if (EndpointHost.Config == null || EndpointHost.Config.Return204NoContentForEmptyResponse)
            {
                if (httpRes.StatusCode == (int)HttpStatusCode.OK)
                {
                    httpRes.StatusCode = (int)HttpStatusCode.NoContent;
                }
            }

            httpRes.SetContentLength(0);
            httpRes.EndRequest();
        }

        public static void LogRequest(this IHttpResponse response, IHttpRequest request, int? statusCode = null)
        {
            try
            {
                if (!EndpointHost.Config.MetadataMap[request.ServicePath].LogCommonRequestInfo)
                    return;

                Dictionary<string, string> additionalInfo = new Dictionary<string, string>()
                {
                    { "ClientIP", request.RemoteIp },
                    { "AbsolutePath", request.GetAbsolutePath() },
                    { "HostAddress", request.GetUrlHostName() },
                    { "ResponseStatus", (statusCode ?? (response.StatusCode <= 0 ? 200 : response.StatusCode)).ToString() }
                };

                string requestType = EndpointHost.Config.MetadataMap[request.ServicePath].FullServiceName;
                if (!string.IsNullOrWhiteSpace(request.OperationName))
                    requestType += "." + request.OperationName;
                additionalInfo["RequestType"] = requestType;

                string appId = request.Headers[ServiceUtils.AppIdHttpHeaderKey];
                if (!string.IsNullOrWhiteSpace(appId))
                    additionalInfo["ClientAppId"] = appId;

                if (request.RequestObject != null && request.RequestObject is IHasMobileRequestHead)
                {
                    IHasMobileRequestHead h5Request = request.RequestObject as IHasMobileRequestHead;
                    if (h5Request.head != null)
                    {
                        Dictionary<string, string> extension = null;
                        if (EndpointHost.Config.MetadataMap[request.ServicePath].LogH5HeadExtensionData)
                        {
                            extension = new Dictionary<string, string>();
                            foreach (ExtensionFieldType item in h5Request.head.extension)
                            {
                                if (!string.IsNullOrWhiteSpace(item.name)
                                    && item.name != ServiceUtils.MobileUserIdExtensionKey && item.name != ServiceUtils.MobileAuthTokenExtensionKey)
                                    extension[item.name] = item.value;
                            }

                            if (extension.Count == 0)
                                extension = null;
                        }
                        additionalInfo["H5Head"] = TypeSerializer.SerializeToString(
                            new
                            {
                                ClientID = h5Request.head.cid,
                                ClientToken = h5Request.head.ctok,
                                ClientVersion = h5Request.head.cver,
                                SystemCode = h5Request.head.syscode,
                                SourceID = h5Request.head.sid,
                                Language = h5Request.head.lang,
                                Extension = extension
                            });
                    }
                }

            }
            catch { }
        }
    }
}