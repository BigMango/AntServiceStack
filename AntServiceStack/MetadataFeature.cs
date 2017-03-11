using System;
using System.Web;
using System.Collections.Generic;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Metadata;

namespace AntServiceStack
{
    public class MetadataFeature : IPlugin
    {
        private static Dictionary<string, Dictionary<string, IHttpHandler>> MetadataHandlers = new Dictionary<string, Dictionary<string, IHttpHandler>>();

        private static IHttpHandler GetMetadataHandler(string servicePath, string metadataType)
        {
            if (servicePath == null || metadataType == null)
                return null;

            Dictionary<string, IHttpHandler> handlers;
            MetadataHandlers.TryGetValue(servicePath, out handlers);

            if (handlers == null)
                return null;

            IHttpHandler handler;
            handlers.TryGetValue(metadataType, out handler);
            return handler;
        }

        public static void RegisterMetadataHandler(string servicePath, string metadataType, IHttpHandler handler)
        {
            if (servicePath == null || metadataType == null || handler == null)
                return;

            Dictionary<string, IHttpHandler> handlers;
            MetadataHandlers.TryGetValue(servicePath, out handlers);
            if (handlers == null)
            {
                handlers = new Dictionary<string, IHttpHandler>();
                MetadataHandlers[servicePath] = handlers;
            }

            handlers[metadataType] = handler;
        }

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

        private IHttpHandler GetHandlerForPathParts(string servicePath, string[] pathParts)
        {
            var pathController = string.Intern(pathParts[0].ToLower());
            if (pathParts.Length == 1)
            {
                if (pathController == "metadata")
                {
                    return GetMetadataHandler(servicePath, pathController) ?? new IndexMetadataHandler(servicePath);
                }

                return null;
            }

            var pathAction = string.Intern(pathParts[1].ToLower());

            if (pathAction != "metadata") return null;

            IHttpHandler handler = GetMetadataHandler(servicePath, pathController);
            if (handler != null)
                return handler;

            switch (pathController)
            {
                case "json":
                    return new JsonMetadataHandler(servicePath);

                case "xml":
                    return new XmlMetadataHandler(servicePath);

                case "jsv":
                    return new JsvMetadataHandler(servicePath);

                case "soap":
                case "soap11":
                    return new Soap11MetadataHandler(servicePath);

                case "operations":

                    return new ActionHandler(servicePath, (httpReq, httpRes) =>
                        EndpointHost.Config.HasAccessToMetadata(httpReq, httpRes)
                            ? EndpointHost.Config.MetadataMap[httpReq.ServicePath].GetOperationDtos()
                            : null, "Operations");

                default:
                    string contentType;
                    if (EndpointHost.ContentTypeFilter.ContentTypeFormats.TryGetValue(pathController, out contentType))
                    {
                        var format = Common.Web.ContentType.GetContentFormat(contentType);
                        return new CustomMetadataHandler(servicePath, contentType, format);
                    }
                    break;
            }
            return null;
        }
    }
}