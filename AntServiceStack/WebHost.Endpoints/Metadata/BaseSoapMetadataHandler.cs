using System;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Xml.Schema;
using AntServiceStack.Common;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.WebHost.Endpoints.Support;
using AntServiceStack.WebHost.Endpoints.Support.Metadata;
using AntServiceStack.WebHost.Endpoints.Support.Metadata.Controls;
using AntServiceStack.WebHost.Endpoints.Utils;

namespace AntServiceStack.WebHost.Endpoints.Metadata
{
    public abstract class BaseSoapMetadataHandler : BaseMetadataHandler, IServiceStackHttpHandler
    {
        protected BaseSoapMetadataHandler(string servicePath)
            : base(servicePath)
        {
            OperationName = GetType().Name.Replace("Handler", "");
        }

        public string OperationName { get; set; }

        public override void Execute(System.Web.HttpContext context)
        {
            IHttpRequest request = new HttpRequestWrapper(ServicePath, OperationName, context.Request);
            IHttpResponse response = new HttpResponseWrapper(context.Response);
            HostContext.InitRequest(request, response);

            if (!EndpointHost.MetadataMap[request.ServicePath].MetadataFeatureEnabled)
            {
                new NotFoundHttpHandler(request.ServicePath).ProcessRequest(request, response, request.OperationName);
                return;
            }
            ProcessRequest(request, response, OperationName);
        }

        public new void ProcessRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            if (!AssertAccess(httpReq, httpRes, httpReq.QueryString["op"])) return;

            var operationTypes = EndpointHost.Config.MetadataMap[httpReq.ServicePath].GetAllTypes();

            using (var sw = new StreamWriter(httpRes.OutputStream))
            {
                var writer = new HtmlTextWriter(sw);
                httpRes.ContentType = "text/html";
                ProcessOperations(writer, httpReq, httpRes);
            }
        }

        protected override void RenderOperations(HtmlTextWriter writer, IHttpRequest httpReq, ServiceMetadata metadata)
        {
            var defaultPage = new IndexOperationsControl
            {
                HttpRequest = httpReq,
                MetadataConfig = EndpointHost.Config.CreateMetadataPagesConfig(metadata),
                Title = metadata.FullServiceName,
                AntServiceStackVersion = metadata.AntServiceStackVersion,
                AntCodeGenVersion = metadata.AntCodeGenVersion,
                OperationNames = metadata.GetAllOperationNames()
            };

            defaultPage.RenderControl(writer);
        }

    }
}