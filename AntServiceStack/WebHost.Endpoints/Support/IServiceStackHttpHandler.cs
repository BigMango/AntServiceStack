using System.Web;
using AntServiceStack.ServiceHost;

namespace AntServiceStack.WebHost.Endpoints.Support
{
    public interface IServiceStackHttpHandler
    {
        void ProcessRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName);
    }
}