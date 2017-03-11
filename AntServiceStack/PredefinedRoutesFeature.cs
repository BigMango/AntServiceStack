using System;
using System.Web;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Support;


namespace AntServiceStack
{
    public class PredefinedRoutesFeature : IPlugin
    {
        public void Register(IAppHost appHost)
        {
            appHost.CatchAllHandlers.Add(ProcessRequest);
        }

        public IHttpHandler ProcessRequest(string httpMethod, string servicePath, string pathInfo, string filePath)
        {
            var pathParts = pathInfo.TrimStart('/').Split('/');
            if (pathParts.Length == 0) return null;
            return GetHandlerForPathParts(servicePath, pathParts);
        }

        private static IHttpHandler GetHandlerForPathParts(string servicePath, string[] pathParts)
        {
            var pathController = string.Intern(pathParts[0].ToLower());
            if (pathParts.Length == 1)
            {
                return null;
            }

            var requestName = string.Intern(pathParts[1]).ToLower(); // aka. operation name
            if (string.IsNullOrWhiteSpace(requestName))
            {
                return null;
                //throw new ArgumentNullException("No operation name was provided");
            }
            if (requestName == "metadata") return null;  // leave for metadata handler


            EndpointHandlerBase rpcHandler = null;
            switch (pathController)
            {
                case "json":
                    rpcHandler = new JsonSyncReplyHandler(servicePath) { RequestName = requestName };
                    break;

                case "xml":
                    rpcHandler = new XmlSyncReplyHandler(servicePath) { RequestName = requestName };
                    break;

                case "jsv":
                    rpcHandler = new JsvSyncReplyHandler(servicePath) { RequestName = requestName };
                    break;

                case "soap":
                case "soap11":
                    rpcHandler = new Soap11Handler(servicePath) { RequestName = requestName };
                    break;

                default:
                    string contentType;
                    if (EndpointHost.ContentTypeFilter.ContentTypeFormats.TryGetValue(pathController, out contentType))
                    {
                        var feature = Common.Web.ContentType.ToFeature(contentType);
                        if (feature == Feature.None) feature = Feature.CustomFormat;

                        rpcHandler = new GenericHandler(servicePath, contentType, EndpointAttributes.Reply, feature)
                        {
                            RequestName = requestName
                        };
                    }
                    break;
            }

            // Centralized operation name validation
            if (rpcHandler != null)
            {
                if (!EndpointHost.Config.MetadataMap[servicePath].HasOperation(requestName))
                    return null;
            }

            return rpcHandler;
        }
    }
}