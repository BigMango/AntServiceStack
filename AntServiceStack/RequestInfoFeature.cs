using System.Web;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Support;

namespace AntServiceStack
{
    public class RequestInfoFeature : IPlugin
    {
        public void Register(IAppHost appHost)
        {
            appHost.CatchAllHandlers.Add(ProcessRequest);
        }

        public IHttpHandler ProcessRequest(string httpMethod, string servicePath, string pathInfo, string filePath)
        {
            var pathParts = pathInfo.TrimStart('/').Split('/');
            return pathParts.Length == 0 ? null : GetHandlerForPathParts(servicePath, pathParts);
        }

        private static IHttpHandler GetHandlerForPathParts(string servicePath, string[] pathParts)
        {
            var pathController = string.Intern(pathParts[0].ToLower());
            return pathController == RequestInfoHandler.RestPath ? new RequestInfoHandler(servicePath) : null;
        }
    }
}