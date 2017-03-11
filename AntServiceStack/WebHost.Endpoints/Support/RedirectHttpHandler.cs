using System;
using System.Net;
using System.Web;
using AntServiceStack.Common;
using AntServiceStack.Common.Extensions;
using AntServiceStack.Common.Web;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;
using EndpointsExtensions = AntServiceStack.WebHost.Endpoints.Extensions;

namespace AntServiceStack.WebHost.Endpoints.Support
{
    public class RedirectHttpHandler
        : IServiceStackHttpHandler, IHttpHandler
    {
        private string _servicePath;

        public string RelativeUrl { get; set; }

        public string AbsoluteUrl { get; set; }

        public RedirectHttpHandler(string servicePath)
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
            if (string.IsNullOrEmpty(RelativeUrl) && string.IsNullOrEmpty(AbsoluteUrl))
                throw new ArgumentNullException("RelativeUrl or AbsoluteUrl");

            if (!string.IsNullOrEmpty(AbsoluteUrl))
            {
                response.StatusCode = (int)HttpStatusCode.Redirect;
                response.AddHeader(HttpHeaders.Location, this.AbsoluteUrl);
            }
            else
            {
                var absoluteUrl = request.GetApplicationUrl();
                if (!string.IsNullOrEmpty(RelativeUrl))
                {
                    if (this.RelativeUrl.StartsWith("/"))
                        absoluteUrl = absoluteUrl.CombineWith(this.RelativeUrl);
                    else if (this.RelativeUrl.StartsWith("~/"))
                        absoluteUrl = absoluteUrl.CombineWith(this.RelativeUrl.Replace("~/", ""));
                    else
                        absoluteUrl = request.AbsoluteUri.CombineWith(this.RelativeUrl);
                }
                response.StatusCode = (int)HttpStatusCode.Redirect;
                response.AddHeader(HttpHeaders.Location, absoluteUrl);
            }

            response.EndHttpHandlerRequest(skipClose: true);
        }

        /// <summary>
        /// ASP.NET requests
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            IHttpRequest request = new EndpointsExtensions.HttpRequestWrapper(_servicePath, typeof(RedirectHttpHandler).Name, context.Request);
            IHttpResponse response = new EndpointsExtensions.HttpResponseWrapper(context.Response);
            HostContext.InitRequest(request, response);
            ProcessRequest(request, response, typeof(RedirectHttpHandler).Name);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}