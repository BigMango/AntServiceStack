using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web;
using AntServiceStack;
using AntServiceStack.Common.Web;
using AntServiceStack.ServiceModel.Serialization;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Support;
using EndpointsExtensions = AntServiceStack.WebHost.Endpoints.Extensions;

namespace AntServiceStack.Plugins.ConfigInfo
{
    public class ConfigInfoPlugin : IPlugin
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

            string pathController = pathParts[0].Trim().ToLower();
            if (pathController == ConfigInfoHandler.RestPath)
                return new ConfigInfoHandler(servicePath);

            return null;
        }
    }
}
