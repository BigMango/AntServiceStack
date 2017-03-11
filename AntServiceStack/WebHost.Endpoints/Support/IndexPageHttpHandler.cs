using System.Net;
using System.Web;
using AntServiceStack.Common.Web;
using AntServiceStack.ServiceHost;
using EndpointsExtensions = AntServiceStack.WebHost.Endpoints.Extensions;

namespace AntServiceStack.WebHost.Endpoints.Support
{
    public class IndexPageHttpHandler
        : IServiceStackHttpHandler, IHttpHandler
    {
        private string _servicePath;

        public IndexPageHttpHandler(string servicePath)
        {
            _servicePath = servicePath;
        }

        /// <summary>
        /// Non ASP.NET requests
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="operationName"></param>
        public void ProcessRequest(IHttpRequest request, IHttpResponse response, string operationName)
        {
            var defaultUrl = EndpointHost.Config.ServiceEndpointsMetadataConfig.DefaultMetadataUri;

            if (request.PathInfo == "/")
            {
                var relativeUrl = defaultUrl.Substring(defaultUrl.IndexOf('/'));
                var absoluteUrl = request.RawUrl.TrimEnd('/') + relativeUrl;
                response.StatusCode = (int)HttpStatusCode.Redirect;
                response.AddHeader(HttpHeaders.Location, absoluteUrl);
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.Redirect;
                response.AddHeader(HttpHeaders.Location, defaultUrl);
            }
        }

        /// <summary>
        /// ASP.NET requests
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            var defaultUrl = EndpointHost.Config.ServiceEndpointsMetadataConfig.DefaultMetadataUri;

            if (context.Request.PathInfo == "/"
                || context.Request.FilePath.EndsWith("/"))
            {
                var relativeUrl = defaultUrl.Substring(defaultUrl.IndexOf('/'));
                var absoluteUrl = context.Request.Url.AbsoluteUri.TrimEnd('/') + relativeUrl;
                context.Response.Redirect(absoluteUrl);
            }
            else
            {
                context.Response.Redirect(defaultUrl);
            }
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}