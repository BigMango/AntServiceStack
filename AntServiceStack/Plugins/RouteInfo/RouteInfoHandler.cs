using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using AntServiceStack;
using AntServiceStack.Common;
using AntServiceStack.Common.Web;
using AntServiceStack.ServiceModel.Serialization;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Support;
using EndpointsExtensions = AntServiceStack.WebHost.Endpoints.Extensions;

namespace AntServiceStack.Plugins.RouteInfo
{
    public class RouteInfoHandler : IHttpHandler, IServiceStackHttpHandler
    {
        public const string RestPath = "_routeinfo";

        string _servicePath;

        public RouteInfoHandler(string servicePath)
        {
            _servicePath = servicePath;
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            IHttpRequest request = new EndpointsExtensions.HttpRequestWrapper(_servicePath, typeof(RouteInfoHandler).Name, context.Request);
            IHttpResponse response = new EndpointsExtensions.HttpResponseWrapper(context.Response);
            HostContext.InitRequest(request, response);
            ProcessRequest(request, response, typeof(RouteInfoHandler).Name);
        }

        public void ProcessRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            if (EndpointHost.ApplyPreRequestFilters(httpReq, httpRes))
                return;

            var response = from o in EndpointHost.Config.MetadataMap[httpReq.ServicePath].Operations
                           orderby o.Name ascending
                           select new
                           {
                               Operation = o.Name,
                               Routes = o.Routes.Select(r => new { Path = r.Path, AllowedVerbs = r.AllowedVerbs ?? "*" }).ToList()
                           };

            httpRes.ContentType = "application/json";
            httpRes.Write(WrappedJsonSerializer.Instance.SerializeToString(response.ToList()));
        }
    }
}
