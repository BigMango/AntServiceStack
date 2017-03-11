using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using AntServiceStack.Common.Web;
using Freeway.Logging;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.WebHost.Endpoints.Support;
using System.Threading.Tasks;
using System.Threading;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Hystrix;

namespace AntServiceStack.WebHost.Endpoints
{
    public class RestHandler
        : EndpointHandlerBase
    {
        public RestHandler(string servicePath)
            : base(servicePath)
        {
            this.HandlerAttributes = EndpointAttributes.Reply;
        }

        public static IRestPath FindMatchingRestPath(string httpMethod, string servicePath, string pathInfo, out string contentType)
        {
            var controller = ServiceManager != null
                ? ServiceManager.ServiceController
                : EndpointHost.Config.ServiceController;

            pathInfo = GetSanitizedPathInfo(pathInfo, out contentType);

            return controller.GetRestPathForRequest(httpMethod, servicePath, pathInfo);
        }

        private static string GetSanitizedPathInfo(string pathInfo, out string contentType)
        {
            contentType = null;
            if (EndpointHost.Config.AllowRouteContentTypeExtensions)
            {
                var pos = pathInfo.LastIndexOf('.');
                if (pos >= 0)
                {
                    var format = pathInfo.Substring(pos + 1);
                    contentType = EndpointHost.ContentTypeFilter.GetFormatContentType(format);
                    if (contentType != null)
                    {
                        pathInfo = pathInfo.Substring(0, pos);
                    }
                }
            }
            return pathInfo;
        }

        public IRestPath GetRestPath(string httpMethod, string servicePath, string pathInfo)
        {
            if (this.RestPath == null)
            {
                string contentType;
                this.RestPath = FindMatchingRestPath(httpMethod, servicePath, pathInfo, out contentType);

                if (contentType != null)
                    ResponseContentType = contentType;
            } 
            return this.RestPath;
        }

        public IRestPath RestPath { get; set; }

        // Set from SSHHF.GetHandlerForPathInfo()
        public string ResponseContentType { get; set; }

        public override void ProcessRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            try
            {
                if (EndpointHost.ApplyPreRequestFilters(httpReq, httpRes)) return;

                var restPath = GetRestPath(httpReq.HttpMethod, httpReq.ServicePath, httpReq.PathInfo);
                if (restPath == null)
                    throw new NotSupportedException("No RestPath found for: " + httpReq.HttpMethod + " " + httpReq.PathInfo);

                operationName = restPath.OperationName;

                var callback = httpReq.GetJsonpCallback();
                var doJsonp = EndpointHost.Config.AllowJsonpRequests
                              && !string.IsNullOrEmpty(callback);

                if (ResponseContentType != null)
                    httpReq.ResponseContentType = ResponseContentType;

                EndpointHost.Config.AssertContentType(httpReq.ResponseContentType);

                var request = GetRequest(httpReq, restPath);
                httpReq.RequestObject = request;
                if (EndpointHost.ApplyRequestFilters(httpReq, httpRes, request, operationName)) return;

                var response = GetResponse(httpReq, httpRes, request);
                if (EndpointHost.ApplyResponseFilters(httpReq, httpRes, response, operationName)) return;

                if (httpReq.ResponseContentType.Contains("jsv") && !string.IsNullOrEmpty(httpReq.QueryString["debug"]))
                {
                    JsvSyncReplyHandler.WriteDebugResponse(httpRes, httpReq, response);
                    return;
                }

                if (doJsonp)
                    httpRes.WriteToResponse(httpReq, response, (callback + "(").ToUtf8Bytes(), ")".ToUtf8Bytes());
                else
                    httpRes.WriteToResponse(httpReq, response);
            }
            catch (Exception ex)
            {
                if (!EndpointHost.Config.WriteErrorsToResponse) throw;
                HandleException(httpReq, httpRes, operationName, ex);
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
        }

        protected override bool PreProcessRequestAsync(IHttpRequest httpReq, IHttpResponse httpRes, string operationName, out object requestObject)
        {
            requestObject = null;

            if (EndpointHost.ApplyPreRequestFilters(httpReq, httpRes))
                return true;

            var restPath = GetRestPath(httpReq.HttpMethod, httpReq.ServicePath, httpReq.PathInfo);
            if (restPath == null)
                throw new NotSupportedException("No RestPath found for: " + httpReq.HttpMethod + " " + httpReq.PathInfo);

            operationName = restPath.OperationName;

            if (ResponseContentType != null)
                httpReq.ResponseContentType = ResponseContentType;

            EndpointHost.Config.AssertContentType(httpReq.ResponseContentType);

            var request = GetRequest(httpReq, restPath);
            httpReq.RequestObject = requestObject = request;
            if (EndpointHost.ApplyRequestFilters(httpReq, httpRes, request, operationName))
                return true;

            return false;
        }

        protected override bool PostProcessRequestAsync(IHttpRequest httpReq, IHttpResponse httpRes, string operationName, object responseObject)
        {
            var callback = httpReq.GetJsonpCallback();
            var doJsonp = EndpointHost.Config.AllowJsonpRequests && !string.IsNullOrEmpty(callback);

            if (EndpointHost.ApplyResponseFilters(httpReq, httpRes, responseObject, operationName))
                return true;

            if (httpReq.ResponseContentType.Contains("jsv") && !string.IsNullOrEmpty(httpReq.QueryString["debug"]))
            {
                JsvSyncReplyHandler.WriteDebugResponse(httpRes, httpReq, responseObject);
                return true;
            }

            if (doJsonp)
            {
                httpRes.WriteToResponse(httpReq, responseObject, (callback + "(").ToUtf8Bytes(), ")".ToUtf8Bytes());
                return true;
            }

            return false;
        }

        public override object GetResponse(IHttpRequest httpReq, IHttpResponse httpRes, object request)
        {
            var requestContentType = ContentType.GetEndpointAttributes(httpReq.ResponseContentType);

            return ExecuteService(request,
                HandlerAttributes | requestContentType | httpReq.GetAttributes(), httpReq, httpRes);
        }

        public void SetResponseContentTypeFromRouteContentTypeExtension(IHttpRequest httpReq)
        {
            string contentType;
            GetSanitizedPathInfo(httpReq.PathInfo, out contentType);
            if (!string.IsNullOrWhiteSpace(contentType))
                httpReq.ResponseContentType = contentType;
        }

        private object GetRequest(IHttpRequest httpReq, IRestPath restPath)
        {
            try
            {
                var requestDto = GetCustomRequestFromBinder(httpReq, restPath.RequestType);
                if (requestDto != null)
                    return requestDto;

                string supportedContentType = httpReq.GetSupportedContentType(ResponseContentType);
                requestDto = CreateContentTypeRequest(httpReq, restPath.RequestType, supportedContentType ?? httpReq.ContentType);

                string contentType;
                var pathInfo = !restPath.IsWildCardPath
                    ? GetSanitizedPathInfo(httpReq.PathInfo, out contentType)
                    : httpReq.PathInfo;
                var requestParams = httpReq.GetRequestParams();
                return restPath.CreateRequestObject(pathInfo, requestParams, requestDto);
            }
            catch (SerializationException e)
            {
                throw new RequestBindingException("Unable to bind request: " + e, e);
            }
            catch (ArgumentException e)
            {
                throw new RequestBindingException("Unable to bind request: " + e, e);
            }
        }

        /// <summary>
        /// Used in Unit tests
        /// </summary>
        /// <returns></returns>
        public override object CreateRequest(IHttpRequest httpReq, string operationName)
        {
            if (this.RestPath == null)
                throw new ArgumentNullException("No RestPath found");

            object request = GetRequest(httpReq, this.RestPath);
            httpReq.RequestObject = request;
            return request;
        }

        public override void SendSyncSLAErrorResponse(IHttpRequest request, IHttpResponse response, HystrixEventType hystrixEvent)
        {
            this.SetResponseContentTypeFromRouteContentTypeExtension(request);

            base.SendSyncSLAErrorResponse(request, response, hystrixEvent);
        }
    }

}
