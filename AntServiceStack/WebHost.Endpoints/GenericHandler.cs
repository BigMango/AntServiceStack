using System;
using AntServiceStack.Common;
using AntServiceStack.Common.Web;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.WebHost.Endpoints.Support;
using System.Threading.Tasks;
using System.Threading;
using AntServiceStack.Common.Utils;
using System.Collections.Generic;

namespace AntServiceStack.WebHost.Endpoints
{
    public class GenericHandler : EndpointHandlerBase
    {
        public GenericHandler(string servicePath, string contentType, EndpointAttributes handlerAttributes, Feature format)
            : base(servicePath)
        {
            this.HandlerContentType = contentType;
            this.ContentTypeAttribute = ContentType.GetEndpointAttributes(contentType);
            this.HandlerAttributes = handlerAttributes;
            this.format = format;
        }

        private Feature format;
        public string HandlerContentType { get; set; }

        public EndpointAttributes ContentTypeAttribute { get; set; }

        public override object CreateRequest(IHttpRequest request, string operationName)
        {
            return GetRequest(request, operationName);
        }

        public override object GetResponse(IHttpRequest httpReq, IHttpResponse httpRes, object request)
        {
            var response = ExecuteService(request,
                HandlerAttributes | httpReq.GetAttributes(), httpReq, httpRes);

            return response;
        }

        public object GetRequest(IHttpRequest httpReq, string operationName)
        {
            var requestType = GetRequestType(operationName, ServicePath);
            AssertOperationExists(operationName, requestType);

            var requestDto = GetCustomRequestFromBinder(httpReq, requestType);
            return requestDto ?? DeserializeHttpRequest(requestType, httpReq, HandlerContentType)
                ?? requestType.CreateInstance();
        }

        public override void ProcessRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            try
            {
                EndpointHost.Config.AssertFeatures(format);

                if (EndpointHost.ApplyPreRequestFilters(httpReq, httpRes)) return;

                httpReq.ResponseContentType = httpReq.GetQueryStringContentType() ?? this.HandlerContentType;
                var callback = httpReq.QueryString["callback"];
                var doJsonp = EndpointHost.Config.AllowJsonpRequests
                              && !string.IsNullOrEmpty(callback);

                var request = CreateRequest(httpReq, operationName);
                httpReq.RequestObject = request;
                if (EndpointHost.ApplyRequestFilters(httpReq, httpRes, request, operationName)) return;

                var response = GetResponse(httpReq, httpRes, request);
                if (EndpointHost.ApplyResponseFilters(httpReq, httpRes, response, operationName)) return;

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

            EndpointHost.Config.AssertFeatures(format);

            if (EndpointHost.ApplyPreRequestFilters(httpReq, httpRes))
                return true;

            httpReq.ResponseContentType = httpReq.GetQueryStringContentType() ?? this.HandlerContentType;

            var request = CreateRequest(httpReq, operationName);
            httpReq.RequestObject = requestObject = request;
            if (EndpointHost.ApplyRequestFilters(httpReq, httpRes, request, operationName))
                return true;

            return false;
        }

        protected override bool PostProcessRequestAsync(IHttpRequest httpReq, IHttpResponse httpRes, string operationName, object responseObject)
        {
            var callback = httpReq.QueryString["callback"];
            var doJsonp = EndpointHost.Config.AllowJsonpRequests && !string.IsNullOrEmpty(callback);

            if (EndpointHost.ApplyResponseFilters(httpReq, httpRes, responseObject, operationName))
                return true;

            if (doJsonp)
            {
                httpRes.WriteToResponse(httpReq, responseObject, (callback + "(").ToUtf8Bytes(), ")".ToUtf8Bytes());
                return true;
            }

            return false;
        }
    }
}
