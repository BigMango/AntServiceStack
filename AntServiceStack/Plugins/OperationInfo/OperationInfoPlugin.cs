using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.WebHost.Endpoints;
using System.Web;

namespace AntServiceStack.Plugins.OperationInfo
{
    public class OperationInfoPlugin : IPlugin
    {
        public void Register(IAppHost appHost)
        {
            appHost.CatchAllHandlers.Add(ResolveHttpHandler);
        }

        public IHttpHandler ResolveHttpHandler(string httpMethod, string servicePath, string pathInfo, string filePath)
        {
            IHttpHandler handler = null;

            if (!string.IsNullOrWhiteSpace(pathInfo))
            {
                int iMaxNumSubString = 1;
                string[] pathParts = pathInfo.TrimStart('/').Split(new char[] { '/' }, iMaxNumSubString, StringSplitOptions.RemoveEmptyEntries);
                if (pathParts.Length == iMaxNumSubString)
                {
                    string pathController = pathParts[0].Trim();
                    if (pathController.Equals(OperationInfoHandler.RestPath, StringComparison.OrdinalIgnoreCase))
                    {
                        handler = new OperationInfoHandler(servicePath);
                    }
                }
            }
            return handler;
        }
    }
}
