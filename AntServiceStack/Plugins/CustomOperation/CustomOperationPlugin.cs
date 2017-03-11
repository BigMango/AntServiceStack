using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;
using AntServiceStack;
using AntServiceStack.Common.Web;
using AntServiceStack.ServiceModel.Serialization;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Support;
using EndpointsExtensions = AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.Plugins.ConfigInfo;

namespace AntServiceStack.Plugins.CustomOperation
{
    public class CustomOperationPlugin : IPlugin
    {
        public void Register(IAppHost appHost)
        {
            appHost.CatchAllHandlers.Add(ResolveHttpHandler);
        }

        public IHttpHandler ResolveHttpHandler(string httpMethod, string servicePath, string pathInfo, string filePath)
        {
            if (string.IsNullOrWhiteSpace(pathInfo))
                return null;

            string[] pathParts = pathInfo.TrimStart('/').Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length < 1)
                return null;

            string restPath = pathParts[0];
            if (CustomOperationHandler.CanHandle(servicePath, restPath))
                return new CustomOperationHandler(servicePath, restPath);

            return null;
        }
    }
}
