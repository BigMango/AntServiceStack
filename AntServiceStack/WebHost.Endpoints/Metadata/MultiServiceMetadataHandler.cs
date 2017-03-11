using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.IO;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Metadata;
using AntServiceStack.WebHost.Endpoints.Support.Metadata.Controls;
using AntServiceStack.ServiceHost;
using HttpRequestWrapper = AntServiceStack.WebHost.Endpoints.Extensions.HttpRequestWrapper;
using HttpResponseWrapper = AntServiceStack.WebHost.Endpoints.Extensions.HttpResponseWrapper;

namespace AntServiceStack.WebHost.Endpoints.Support
{
    public class MultiServiceMetadataHandler : HttpHandlerBase, IServiceStackHttpHandler
    {
        public override void Execute(HttpContext context)
        {
            ProcessRequest(new HttpRequestWrapper(ServiceMetadata.EmptyServicePath, null, context.Request), new HttpResponseWrapper(context.Response), null);
        }

        public void ProcessRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            var enabledMetadatas = EndpointHost.MetadataMap.Where(item => item.Value.MetadataFeatureEnabled);
            if (enabledMetadatas.Count() == 0)
            {
                new NotFoundHttpHandler(httpReq.ServicePath).ProcessRequest(httpReq, httpRes, operationName);
                return;
            }
            else if (enabledMetadatas.Count() == 1 && enabledMetadatas.ElementAt(0).Key == ServiceMetadata.EmptyServicePath)
            {
                var redirectHttpHandler = AntServiceStackHttpHandlerFactory.GetRedirectHandler(HttpContext.Current.Request.GetApplicationUrl(), httpReq.ServicePath);
                redirectHttpHandler.ProcessRequest(HttpContext.Current);
                return;
            }

            using (var sw = new StreamWriter(httpRes.OutputStream))
            {
                var writer = new HtmlTextWriter(sw);
                httpRes.ContentType = "text/html";
                RenderServices(writer, httpReq);
            }
        }

        protected void RenderServices(HtmlTextWriter writer, IHttpRequest httpReq)
        {
            Dictionary<string, string> serviceData = EndpointHost.Config.MetadataMap.Where(item => item.Value.MetadataFeatureEnabled).ToDictionary(p => p.Key, p => p.Value.FullServiceName);
            var defaultPage = new IndexServicesControl
            {
                HttpRequest = httpReq,
                AntServiceStackVersion = this.GetType().Assembly.GetName().Version.ToString(),
                ServiceData = serviceData
            };

            defaultPage.RenderControl(writer);
        }
    }
}
