using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using AntServiceStack;
using AntServiceStack.Common.Web;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStackSwagger.SwaggerUi;

namespace AntServiceStackSwagger
{
    public class SwaggerServicePlugin : IPlugin
    {
        private SwaggerUiConfig _swaggerUiConfig;
        public void Register(IAppHost appHost)
        {
            if (_swaggerUiConfig == null)
            {
                _swaggerUiConfig = new SwaggerUiConfig(appHost.Config);
            }
            else
            {
                _swaggerUiConfig.HostConfig = appHost.Config;
            }
            appHost.Config.RawHttpHandlers.Add(ResolveHttpHandler);
            appHost.GetPlugin<MetadataFeature>()
               .AddPluginLink(SwaggerUiHandler.RESOURCE_PATH + "/", "Swagger UI");
        }

        private IHttpHandler ResolveHttpHandler(IHttpRequest request)
        {
            if (request.HttpMethod == HttpMethods.Post )
                return null;

            var paths = GetPathController(request);
            if (paths == null) return null;

            if (paths.Contains(SwaggerUiHandler.RESOURCE_PATH))
            {
                return new SwaggerUiHandler(_swaggerUiConfig, request.ServicePath);
            }else if (paths.Contains(SwaggerResourcesService.RESOURCE_PATH))
            {
                return new SwaggerResourcesService(_swaggerUiConfig,request.ServicePath);
            }
            else if (paths.Contains(SwaggerApiService.RESOURCE_PATH))
            {
                return new SwaggerApiService(_swaggerUiConfig, request.ServicePath);
            }
            return null;
        }
        private string[] GetPathController(IHttpRequest request)
        {
            var pathParts = request.PathInfo.TrimStart('/').Split('/');
            return pathParts;
        }

        public SwaggerServicePlugin(SwaggerUiConfig swaggerUiConfig = null)
        {
            _swaggerUiConfig = swaggerUiConfig;
        }
    }
}
