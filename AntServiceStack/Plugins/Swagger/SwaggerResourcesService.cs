//-----------------------------------------------------------------------
// <copyright file="AppHostNewExtensions.cs" company="Company">
// Copyright (C) Company. All Rights Reserved.
// </copyright>
// <author>nainaigu</author>
// <summary></summary>
//-----------------------------------------------------------------------

using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using AntServiceStack;
using AntServiceStack.Common;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.WebHost.Endpoints.Support;
using AntServiceStackSwagger.SwaggerUi;
    
namespace AntServiceStackSwagger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


    /// <summary>
    /// 
    /// </summary>
    //[CServiceInterface(CodeGeneratorVersion = "2.0.0.0",ServiceName = "resources", ServiceNamespace="swagger")]
    public class SwaggerResourcesService : IServiceStackHttpHandler, IHttpHandler
    {
        private readonly string _servicePath;
        private readonly Regex resourcePathCleanerRegex = new Regex(@"/[^\/\{]*", RegexOptions.Compiled);
        internal static Action<SwaggerResourcesResponse> ResourcesResponseFilter { get; set; }
        internal const string RESOURCE_PATH = "swagger-resources";
        private readonly SwaggerUiConfig _config;
        public SwaggerResourcesService(SwaggerUiConfig config, string servicePath)
        {
            _config = config;
            _servicePath = servicePath;
        }

        public void ProcessRequest(IHttpRequest request, IHttpResponse response, string operationName)
        {
            try
            {
                response.ContentType = "application/json";
                var basePath = request.GetBaseUrl();
                var result = new SwaggerResourcesResponse
                {
                    BasePath = basePath,
                    Apis = new List<SwaggerResourceRef>(),
                    ApiVersion = _config.ApiVersion,
                    Info = new SwaggerInfo
                    {
                        Title = _config.Title ?? "Ant-SOA-API",
                    }
                };

                if (_config.UseBasicAuth)
                {
                    var basicAuth = request.GetBasicAuthUserAndPassword();
                    if (basicAuth == null)
                    {
                        result.Info.Title = "Auth Error";
                        response.Write(result.ToJson());
                        response.EndRequest(true);
                        return;
                    }
                    else
                    {
                        var userName = basicAuth.Value.Key;
                        var password = basicAuth.Value.Value;
                        var localAuth = _config.GetLocalAuthModel();
                        if (!localAuth.UserName.Equals(userName) && !localAuth.Password.Equals(password))
                        {
                            result.Info.Title = "Auth Error";
                            response.Write(result.ToJson());
                            response.EndRequest(true);
                            return;
                        }
                    }
                }
                result.Apis.Add(new SwaggerResourceRef
                {
                    Path = request.ResolveAbsoluteUrl("~/" + SwaggerApiService.RESOURCE_PATH),
                    Description = _config.HostConfig.MetadataMap.FirstOrDefault().Value.ServiceName
                });

                result.Apis = result.Apis.OrderBy(a => a.Path).ToList();
                if (ResourcesResponseFilter != null)
                    ResourcesResponseFilter(result);

                response.Write(result.ToJson());
                response.EndRequest(true);
            }
            catch (Exception)
            {
                
                response.EndRequestWithNoContent();
            }
        }

        protected void CreateRestPaths(List<SwaggerResourceRef> apis, string name, IHttpRequest request)
        {
            var serviceController = _config.HostConfig.ServiceController as ServiceController;
            if (serviceController == null)
            {
                return;
            }
            if (!serviceController.RestPathMap.ContainsKey(name))
            {
                return;
            }
            var map = serviceController.RestPathMap[name];

            foreach (KeyValuePair<string, List<RestPath>> item in map)
            {

                string basePath = string.Empty;
                string desc = string.Empty;
                var pathInfo = item.Value.FirstOrDefault();
                if (pathInfo != null)
                {
                    desc = pathInfo.Summary;
                    basePath = pathInfo.Path;
                    if (string.IsNullOrEmpty(basePath))
                        continue;
                }

                apis.Add(new SwaggerResourceRef
                {
                    Path = request.ResolveAbsoluteUrl("~/" + SwaggerApiService.RESOURCE_PATH),
                    Description = desc
                });
            }

        }

        public void ProcessRequest(HttpContext context)
        {
            //if (_config.UseBasicAuth &&  context.Response.StatusCode == HttpNotAuthorizedStatusCode)
            //{
            //    context.Response.AddHeader(HttpWWWAuthenticateHeader, "Basic realm =\"" + Realm + "\"");
            //}

            var request = context.Request;
            var response = context.Response;
            var httpReq = new AntServiceStack.WebHost.Endpoints.Extensions.HttpRequestWrapper(_servicePath, typeof(SwaggerResourcesService).Name, request);
            var httpRes = new AntServiceStack.WebHost.Endpoints.Extensions.HttpResponseWrapper(response);
            HostContext.InitRequest(httpReq, httpRes);
            ProcessRequest(httpReq, httpRes, null);
        }

        public bool IsReusable
        {
            get { return false; }
        }


        //#region Basic Auth
        //public const String HttpAuthorizationHeader = "Authorization";			// HTTP1.1 Authorization header 
        //public const String HttpBasicSchemeName = "Basic";						// HTTP1.1 Basic Challenge Scheme Name 
        //public const Char HttpCredentialSeparator = ':';						// HTTP1.1 Credential username and password separator 
        //public const int HttpNotAuthorizedStatusCode = 401;						// HTTP1.1 Not authorized response status code 
        //public const String HttpWWWAuthenticateHeader = "WWW-Authenticate";		// HTTP1.1 Basic Challenge Scheme Name 
        //public const String Realm = "";                                         // HTTP.1.1 Basic Challenge Realm 
        //public string host = string.Empty;

        //#endregion
    }




    [DataContract]
    public class SwaggerResources
    {
        [DataMember(Name = "apiKey")]
        public string ApiKey { get; set; }
    }

    [DataContract]
    public class SwaggerResourcesResponse
    {
        [DataMember(Name = "swaggerVersion")]
        public string SwaggerVersion
        {
            get { return "1.2"; }
        }
        [DataMember(Name = "apis")]
        public List<SwaggerResourceRef> Apis { get; set; }
        [DataMember(Name = "apiVersion")]
        public string ApiVersion { get; set; }
        [DataMember(Name = "basePath")]
        public string BasePath { get; set; }
        [DataMember(Name = "info")]
        public SwaggerInfo Info { get; set; }
    }

    [DataContract]
    public class SwaggerInfo
    {
        [DataMember(Name = "title")]
        public string Title { get; set; }
        [DataMember(Name = "description")]
        public string Description { get; set; }
        [DataMember(Name = "termsOfServiceUrl")]
        public string TermsOfServiceUrl { get; set; }
        [DataMember(Name = "contact")]
        public string Contact { get; set; }
        [DataMember(Name = "license")]
        public string License { get; set; }
        [DataMember(Name = "licenseUrl")]
        public string LicenseUrl { get; set; }
    }

    [DataContract]
    public class SwaggerResourceRef
    {
        [DataMember(Name = "path")]
        public string Path { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }
    }
}