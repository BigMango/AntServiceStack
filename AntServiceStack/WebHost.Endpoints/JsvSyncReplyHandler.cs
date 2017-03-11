using System;
using System.Text;
using AntServiceStack.Common.Web;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;
using AntServiceStack.WebHost.Endpoints.Extensions;

namespace AntServiceStack.WebHost.Endpoints
{
    public class JsvSyncReplyHandler : GenericHandler
    {
        public JsvSyncReplyHandler(string servicePath)
            : base(servicePath, ContentType.JsvText, EndpointAttributes.Reply | EndpointAttributes.Jsv, Feature.Jsv) { }

        private static void WriteDebugRequest(IRequestContext requestContext, object dto, IHttpResponse httpRes)
        {
            var bytes = Encoding.UTF8.GetBytes(dto.SerializeAndFormat());
            httpRes.OutputStream.Write(bytes, 0, bytes.Length);
        }

        public override void ProcessRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            var isDebugRequest = httpReq.RawUrl.ToLower().Contains("debug");
            if (!isDebugRequest)
            {
                base.ProcessRequest(httpReq, httpRes, operationName);
                return;
            }

            try
            {
                var request = CreateRequest(httpReq, operationName);
                httpReq.RequestObject = request;

                var response = ExecuteService(request,
                    HandlerAttributes | httpReq.GetAttributes(), httpReq, httpRes);

                WriteDebugResponse(httpRes, httpReq, response);
            }
            catch (Exception ex)
            {
                if (!EndpointHost.Config.WriteErrorsToResponse) throw;
                HandleException(httpReq, httpRes, operationName, ex);
            }
        }

        public static void WriteDebugResponse(IHttpResponse httpRes, IHttpRequest httpReq, object response)
        {
            httpRes.WriteToResponse(httpReq, response, WriteDebugRequest, new SerializationContext(ContentType.PlainText), null, null);
            httpRes.EndRequest();
        }
    }
}