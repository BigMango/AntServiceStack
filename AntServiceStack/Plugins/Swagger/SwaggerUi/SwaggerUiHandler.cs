//-----------------------------------------------------------------------
// <copyright file="SwaggerUiHandler.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------

using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AntServiceStack;
using AntServiceStack.Common;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.WebHost.Endpoints.Support;

namespace AntServiceStackSwagger.SwaggerUi
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 静态资源
    /// </summary>
    public class SwaggerUiHandler : IServiceStackHttpHandler, IHttpHandler
    {
        private readonly string _servicePath;
        private readonly SwaggerUiConfig _config;
        internal const string RESOURCE_PATH = "swagger-ui";
        public SwaggerUiHandler(SwaggerUiConfig config,string servicePath)
        {
            _config = config;
            _servicePath = servicePath;
        }

        public void ProcessRequest(IHttpRequest request, IHttpResponse response, string operationName)
        {
            try
            {

                var isIndex = false;
                var swaggerUiProvider = _config.GetSwaggerUiProvider();
                var swaggerUrl = request.PathInfo.Substring(request.PathInfo.IndexOf(RESOURCE_PATH, StringComparison.Ordinal) + RESOURCE_PATH.Length);
                if (swaggerUrl.Length == 0)
                {
                    response.Redirect(request.ResolveAbsoluteUrl("~/swagger-ui/index.html"));
                    return;
                }
                if (swaggerUrl.Equals("/") || swaggerUrl.Equals("/index.html") ||  swaggerUrl.Equals("/index"))
                {
                    swaggerUrl = "index.html";
                    isIndex = true;
                }
                if (swaggerUrl.StartsWith("/"))
                {
                    swaggerUrl = swaggerUrl.Substring(1);
                }
                var webAsset = swaggerUiProvider.GetAsset(swaggerUrl);
                response.ContentType = webAsset.MediaType;

                if (isIndex)
                {
                    var resourcesUrl = request.ResolveAbsoluteUrl("~/swagger-resources");
                    var replaceDic = new Dictionary<string,string>();
                    replaceDic.Add("http://petstore.swagger.io/v2/swagger.json", resourcesUrl);
                    replaceDic.Add("ApiDocs",operationName );
                    replaceDic.Add("{LogoUrl}", request.ResolveAbsoluteUrl("~/swagger-ui/images/logo-24.png"));
                    if (_config.InjectJs)
                    {
                        var injectJsHtml = swaggerUiProvider.GetAsset("patch.js");
                        if (injectJsHtml != null)
                        {
                            replaceDic.Add("</body>", "<script type='text/javascript'>" + injectJsHtml.Stream.GetText() + "</script></body>");
                        }
                    }

                    replaceDic.Add("\"your-realms\"", string.IsNullOrEmpty(_config.H5LoginUrl) ? "getRealm() " : "\"" + _config.H5LoginUrl + "\"");

                    var stream = webAsset.Stream.FindAndReplace(replaceDic);
                    stream.WriteTo(response.OutputStream);
                    response.EndRequest(false);
                    return;
                }
                webAsset.Stream.WriteTo(response.OutputStream);
                response.EndRequest(false);
            }
            catch (AssetNotFound ex)
            {
                response.Write(ex.Message);
                response.EndRequest(true);
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;
            var httpReq = new AntServiceStack.WebHost.Endpoints.Extensions.HttpRequestWrapper(_servicePath, typeof(SwaggerUiHandler).Name, request);
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