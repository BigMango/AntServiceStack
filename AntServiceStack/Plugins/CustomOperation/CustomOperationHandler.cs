using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using AntServiceStack;
using AntServiceStack.Common;
using AntServiceStack.Common.Web;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Types;
using AntServiceStack.ServiceModel.Serialization;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Support;
using AntServiceStack.Plugins.WhiteList;
using EndpointsExtensions = AntServiceStack.WebHost.Endpoints.Extensions;

namespace AntServiceStack.Plugins.CustomOperation
{
    public class CustomOperationHandler : IHttpHandler, IServiceStackHttpHandler
    {
        static readonly Dictionary<string, Dictionary<string, ICustomOperation>> CustomOperations = new Dictionary<string, Dictionary<string, ICustomOperation>>();

        public static void RegisterCustomOperation(ICustomOperation customOperation)
        {
            RegisterCustomOperation(customOperation, null);
        }

        public static void RegisterCustomOperation(ICustomOperation customOperation, string servicePath)
        {
            if (customOperation == null || string.IsNullOrWhiteSpace(customOperation.OperationRestPath))
                return;

            servicePath = string.IsNullOrWhiteSpace(servicePath) ? ServiceMetadata.DefaultServicePath : servicePath.Trim().ToLower();
            if (!EndpointHost.Config.MetadataMap.ContainsKey(servicePath))
                throw new Exception(string.Format("Make sure the service path '{0}' is existing in the apphost.", servicePath));

            string restPath = customOperation.OperationRestPath.Trim().ToLower();
            if (!CustomOperations.ContainsKey(servicePath))
                CustomOperations[servicePath] = new Dictionary<string, ICustomOperation>();
            CustomOperations[servicePath][restPath] = customOperation;

            WhiteListPlugin.ExcludePathController(restPath, servicePath);
        }

        internal static bool CanHandle(string servicePath, string restPath)
        {
            if (servicePath == null || string.IsNullOrWhiteSpace(restPath))
                return false;

            servicePath = servicePath.Trim().ToLower();
            restPath = restPath.Trim().ToLower();
            return CustomOperations.ContainsKey(servicePath) && CustomOperations[servicePath].ContainsKey(restPath);
        }

        protected readonly string ServicePath;
        protected readonly string RestPath;

        public CustomOperationHandler(string servicePath, string restPath)
        {
            ServicePath = servicePath;
            RestPath = restPath.Trim().ToLower();
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            IHttpRequest request = new EndpointsExtensions.HttpRequestWrapper(ServicePath, typeof(CustomOperationHandler).Name, context.Request);
            IHttpResponse response = new EndpointsExtensions.HttpResponseWrapper(context.Response);
            HostContext.InitRequest(request, response);
            ProcessRequest(request, response, typeof(CustomOperationHandler).Name);
        }

        public void ProcessRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            DataFormat format = httpReq.ContentType.ToDataFormat();
            if (format == DataFormat.NotSupported)
                format = httpReq.QueryString["format"].ToDataFormat();
            HttpMethodEnum method = httpReq.HttpMethod.ToHttpMethod();
            ICustomOperation customOperation = CustomOperations[httpReq.ServicePath][RestPath];
            AckCodeType? ack = AckCodeType.Success;
            try
            {
                if (method == HttpMethodEnum.NotSupported)
                    throw new NotSupportedException("HTTP Method " + method + " is not supported.");
                if (format == DataFormat.NotSupported)
                    throw new NotSupportedException("Data Transfer Format is not supported.");

                if (EndpointHost.ApplyPreRequestFilters(httpReq, httpRes))
                {
                    ack = null;
                    return;
                }

                if (customOperation.RequestDTOType != null)
                {
                    if (method == HttpMethodEnum.POST || method == HttpMethodEnum.PUT)
                    {
                        if (httpReq.InputStream.Length > 0)
                        {
                            using (Stream stream = httpReq.InputStream)
                            {
                                httpReq.RequestObject = GeneralSerializer.Deserialize(customOperation.RequestDTOType, stream, format);
                            }
                        }
                    }
                    else
                    {
                        bool hasOnlyFormatParam = httpReq.QueryString.Count == 1 && httpReq.QueryString["format"] != null;
                        if (httpReq.QueryString.Count > 0 && !hasOnlyFormatParam)
                            httpReq.RequestObject = GeneralSerializer.DeserializeFromQueryString(customOperation.RequestDTOType, httpReq.QueryString);
                    }
                }

                if (!customOperation.IsValidRequest(httpReq, httpReq.RequestObject))
                    throw new UnauthorizedAccessException("Not allowed.");

                httpRes.ContentType = format.ToContentType();
                object responseObject = customOperation.ExecuteOperation(httpReq, httpReq.RequestObject);
                if (responseObject == null)
                    return;

                if (EndpointHost.Config.MetadataMap[ServicePath].UseChunkedTransferEncoding)
                {
                    httpRes.AddHeader(ServiceUtils.ResponseStatusHttpHeaderKey, ack.Value.ToString());
                    EndpointsExtensions.HttpResponseExtensions.UseChunkedTransferEncoding(httpRes);
                }

                using (Stream stream = httpRes.OutputStream)
                {
                    GeneralSerializer.Serialize(responseObject, stream, format);
                }
            }
            catch (Exception ex)
            {
                ack = AckCodeType.Failure;
                ErrorUtils.LogError("Custom Operation Error", httpReq, ex, true, "FXD300008");
                httpRes.StatusCode = EndpointsExtensions.HttpResponseExtensions.ToStatusCode(ex);
            }
            finally
            {
                if (ack.HasValue)
                {
                    if (!EndpointsExtensions.HttpResponseExtensions.UsedChunkedTransferEncoding(httpRes))
                        httpRes.AddHeader(ServiceUtils.ResponseStatusHttpHeaderKey, ack.Value.ToString());

                    httpRes.LogRequest(httpReq);
                }

                HostContext.Instance.EndRequest();
            }
        }
    }
}
