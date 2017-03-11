using System.Web;
using AntServiceStack.Common;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints.Support;

namespace AntServiceStack.Plugins.Consul.Services
{
    public class ConsulHeartbeatHandler : IServiceStackHttpHandler, IHttpHandler
    {
        private readonly string _servicePath;

        public ConsulHeartbeatHandler(string servicePath)
        {
            _servicePath = servicePath;
        }
        public void ProcessRequest(IHttpRequest request, IHttpResponse response, string operationName)
        {
           //直接不处理 默认返回 200 OK
        }

        public void ProcessRequest(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;
            var httpReq = new AntServiceStack.WebHost.Endpoints.Extensions.HttpRequestWrapper(_servicePath, typeof(ConsulHeartbeatHandler).Name, request);
            var httpRes = new AntServiceStack.WebHost.Endpoints.Extensions.HttpResponseWrapper(response);
            HostContext.InitRequest(httpReq, httpRes);
            ProcessRequest(httpReq, httpRes, null);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
