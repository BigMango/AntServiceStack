using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Xml.Linq;

using Freeway.Logging;

using AntServiceStack.Common.ServiceModel;
using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Web;
using AntServiceStack.Configuration;
using AntServiceStack.ServiceHost;
using AntServiceStack.ServiceModel;
using AntServiceStack.ServiceModel.Serialization;
using AntServiceStack.Text;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.WebHost.Endpoints.Support;
using AntServiceStack.WebHost.Endpoints.Metadata.Config;

namespace AntServiceStack.WebHost.Endpoints
{
    public class EndpointHostConfig
    {
        public static bool SkipPathValidation = false;
        /// <summary>
        /// Use: \[Route\("[^\/]  regular expression to find violating routes in your sln
        /// </summary>
        public static bool SkipRouteValidation = false;

        public static string ServiceStackPath = null;

        private static EndpointHostConfig instance;
        public static EndpointHostConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EndpointHostConfig
                    {
                        EnableAccessRestrictions = true,
                        WebHostPhysicalPath = "~".MapServerPath(),
                        ServiceStackHandlerFactoryPath = null,
                        MetadataRedirectPath = null,
                        DefaultContentType = ContentType.Html,
                        AllowJsonpRequests = true,
                        AllowRouteContentTypeExtensions = true,
                        AllowNonHttpOnlyCookies = false,
                        DebugMode = false,
                        DefaultDocuments = new List<string> {
							"default.htm",
							"default.html",
							"default.cshtml",
							"default.md",
							"index.htm",
							"index.html",
							"default.aspx",
							"default.ashx",
						},
                        GlobalResponseHeaders = new Dictionary<string, string> { { "X-Powered-By", Env.ServerUserAgent } },
                        IgnoreFormatsInMetadata = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase),
                        AllowFileExtensions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
						{
							"js", "css", "htm", "html", "shtm", "txt", "xml", "rss", "csv", "pdf",  
							"jpg", "jpeg", "gif", "png", "bmp", "ico", "tif", "tiff", "svg",
							"avi", "divx", "m3u", "mov", "mp3", "mpeg", "mpg", "qt", "vob", "wav", "wma", "wmv", 
							"flv", "xap", "xaml", "ogg", "mp4", "webm", "eot", "ttf", "woff"
						},
                        DebugAspNetHostEnvironment = Env.IsMono ? "FastCGI" : "IIS7",
                        DebugHttpListenerHostEnvironment = Env.IsMono ? "XSP" : "WebServer20",
                        EnableFeatures = Feature.All,
                        WriteErrorsToResponse = true,
                        ReturnsInnerException = true,
                        HtmlReplaceTokens = new Dictionary<string, string>(),
                        AddMaxAgeForStaticMimeTypes = new Dictionary<string, TimeSpan> {
							{ "image/gif", TimeSpan.FromHours(1) },
							{ "image/png", TimeSpan.FromHours(1) },
							{ "image/jpeg", TimeSpan.FromHours(1) },
						},
                        AppendUtf8CharsetOnContentTypes = new HashSet<string> { ContentType.Json, },
                        RawHttpHandlers = new List<Func<IHttpRequest, IHttpHandler>>(),
                        OnlySendSessionCookiesSecurely = false,
                        RestrictAllCookiesToDomain = null,
                        DefaultJsonpCacheExpiration = new TimeSpan(0, 20, 0),
                        MetadataVisibility = EndpointAttributes.Any,
                        Return204NoContentForEmptyResponse = true,
                        AllowPartialResponses = true,
                        IgnoreWarningsOnPropertyNames = new List<string>() {
                            "format", "callback", "debug", "_"
                        },
                        ServicePaths = new List<string>(),
                        FallbackRestPaths = new Dictionary<string,FallbackRestPathDelegate>(),
                        WebHostIP = ServiceUtils.HostIP
                    };

                    if (!string.IsNullOrWhiteSpace(ServiceStackPath))
                        instance.SetServiceStackHandlerFactoryPath(ServiceStackPath);
                    else
                    {
                        InferHttpHandlerPath();

                        if (instance.ServiceStackHandlerFactoryPath == null)
                            instance.MetadataRedirectPath = "metadata";
                    }

                    instance.ServiceEndpointsMetadataConfig = ServiceEndpointsMetadataConfig.Create(instance.ServiceStackHandlerFactoryPath);

                   
                }

                return instance;
            }
        }

        public static void Reset()
        {
            instance = null;
        }

        private static System.Configuration.Configuration GetAppConfig()
        {
            // Web.Config
            try
            {
                return WebConfigurationManager.OpenWebConfiguration("~/");
            }
            catch { }

            // App.Config
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
                return ConfigurationManager.OpenExeConfiguration(entryAssembly.Location);

            return null;
        }

        private static void InferHttpHandlerPath()
        {
            try
            {
                var config = GetAppConfig();
                if (config == null) return;

                SetPathsFromConfiguration(config, null);

                if (instance.MetadataRedirectPath == null)
                {
                    foreach (ConfigurationLocation location in config.Locations)
                    {
                        SetPathsFromConfiguration(location.OpenConfiguration(), (location.Path ?? "").ToLower());

                        if (instance.MetadataRedirectPath != null) { break; }
                    }
                }

                if (!SkipPathValidation && instance.MetadataRedirectPath == null)
                {
                    throw new ConfigurationErrorsException(
                        "Unable to infer AntServiceStack's <httpHandler.Path/> from the Web.Config\n"
                        + "Check with http://www.servicestack.net/ServiceStack.Hello/ to ensure you have configured ServiceStack properly.\n"
                        + "Otherwise you can explicitly set your httpHandler.Path by setting: EndpointHostConfig.ServiceStackPath");
                }
            }
            catch (Exception) { }
        }

        private static void SetPathsFromConfiguration(System.Configuration.Configuration config, string locationPath)
        {
            if (config == null)
                return;

            //standard config
            var handlersSection = config.GetSection("system.web/httpHandlers") as HttpHandlersSection;
            if (handlersSection != null)
            {
                for (var i = 0; i < handlersSection.Handlers.Count; i++)
                {
                    var httpHandler = handlersSection.Handlers[i];
                    if (!httpHandler.Type.StartsWith("AntServiceStack"))
                        continue;

                    SetPaths(httpHandler.Path, locationPath);
                    break;
                }
            }

            //IIS7+ integrated mode system.webServer/handlers
            var pathsNotSet = instance.MetadataRedirectPath == null;
            if (pathsNotSet)
            {
                var webServerSection = config.GetSection("system.webServer");
                if (webServerSection != null)
                {
                    var rawXml = webServerSection.SectionInformation.GetRawXml();
                    if (!string.IsNullOrEmpty(rawXml))
                    {
                        SetPaths(ExtractHandlerPathFromWebServerConfigurationXml(rawXml), locationPath);
                    }
                }

                //In some MVC Hosts auto-inferencing doesn't work, in these cases assume the most likely default of "/api" path
                pathsNotSet = instance.MetadataRedirectPath == null;
                if (pathsNotSet)
                {
                    var isMvcHost = Type.GetType("System.Web.Mvc.Controller") != null;
                    if (isMvcHost)
                    {
                        SetPaths("api", null);
                    }
                }
            }
        }

        private static void SetPaths(string handlerPath, string locationPath)
        {
            if (handlerPath == null) return;

            if (locationPath == null)
            {
                handlerPath = handlerPath.Replace("*", String.Empty);
            }

            instance.SetServiceStackHandlerFactoryPath(locationPath ?? (string.IsNullOrEmpty(handlerPath) ? null : handlerPath));
        }

        private static string ExtractHandlerPathFromWebServerConfigurationXml(string rawXml)
        {
            XElement handlersElement = XDocument.Parse(rawXml).Root.Element("handlers");
            if (handlersElement == null)
                return null;
            return handlersElement.Descendants("add").Where(handler => EnsureHandlerTypeAttribute(handler).StartsWith("AntServiceStack"))
                .Select(handler => handler.Attribute("path").Value).FirstOrDefault();
        }

        private static string EnsureHandlerTypeAttribute(XElement handler)
        {
            if (handler.Attribute("type") != null && !string.IsNullOrEmpty(handler.Attribute("type").Value))
            {
                return handler.Attribute("type").Value;
            }
            return string.Empty;
        }

        private ServiceManager _serviceManager;

        /// <summary>
        /// 服务初始化 route controller action
        /// </summary>
        public ServiceManager ServiceManager
        {
            get
            {
                return _serviceManager;
            }
            set
            {
                _serviceManager = value;
                ServicePaths = _serviceManager.MetadataMap.Keys.ToList();
                HostMultipleServices = ServicePaths.Count > 1 || ServicePaths.Count == 1 && ServicePaths[0] != ServiceMetadata.EmptyServicePath;
            }
        }

        public Dictionary<string, ServiceMetadata> MetadataMap { get { return ServiceManager.MetadataMap; } }
        public IServiceController ServiceController { get { return ServiceManager.ServiceController; } }
        public List<string> ServicePaths { get; private set; }
        public bool HostMultipleServices { get; private set; }

        private EndpointAttributes metadataVisibility;
        public EndpointAttributes MetadataVisibility
        {
            get { return metadataVisibility; }
            set { metadataVisibility = value.ToAllowedFlagsSet(); }
        }

        public string DefaultContentType { get; set; }
        public bool AllowJsonpRequests { get; set; }
        public bool AllowRouteContentTypeExtensions { get; set; }

        /// <summary>
        /// 设置Debugm模式
        /// </summary>
        public bool DebugMode { get; set; }
        public bool DebugOnlyReturnRequestInfo { get; set; }
        public string DebugAspNetHostEnvironment { get; set; }
        public string DebugHttpListenerHostEnvironment { get; set; }
        public List<string> DefaultDocuments { get; private set; }

        public List<string> IgnoreWarningsOnPropertyNames { get; private set; }

        public HashSet<string> IgnoreFormatsInMetadata { get; set; }

        public HashSet<string> AllowFileExtensions { get; set; }

        /// <summary>
        /// 请求的完整地址
        /// </summary>
        public string WebHostUrl { get; set; }
        /// <summary>
        /// 请求的端口
        /// </summary>
        public string WebHostPort { get; set; }
        /// <summary>
        /// 服务器IP
        /// </summary>
        public string WebHostIP { get; set; }

        /// <summary>
        /// 服务磁盘目录
        /// </summary>
        public string WebHostPhysicalPath { get; set; }

        /// <summary>
        /// 请求的虚拟目录
        /// </summary>
        public string ServiceStackHandlerFactoryPath { get; set; }

        /// <summary>
        /// 是否开启Consul服务发现
        /// </summary>
        public bool UseConsulDiscovery { get; set; }

        public string DefaultRedirectPath { get; set; }
        public string MetadataRedirectPath { get; set; }

        public ServiceEndpointsMetadataConfig ServiceEndpointsMetadataConfig { get; private set; }
        public bool EnableAccessRestrictions { get; set; }

        private bool _useBclJsonSerializers;
        public bool UseBclJsonSerializers
        {
            get
            {
                return _useBclJsonSerializers;
            }
            set
            {
                _useBclJsonSerializers = value;
                WrappedJsonSerializer.Instance.UseBcl = _useBclJsonSerializers;
                WrappedJsonDeserializer.Instance.UseBcl = _useBclJsonSerializers;
            }
        }

        private bool _useDataContractXmlSerializers;
        public bool UseDataContractXmlSerializers
        {
            get
            {
                return _useDataContractXmlSerializers;
            }
            set
            {
                _useDataContractXmlSerializers = value;
                WrappedXmlSerializer.UseDataContract = _useDataContractXmlSerializers;
            }
        }

        public Dictionary<string, string> GlobalResponseHeaders { get; set; }
        public Feature EnableFeatures { get; set; }
        public bool ReturnsInnerException { get; set; }
        public bool WriteErrorsToResponse { get; set; }

        public Dictionary<string, string> HtmlReplaceTokens { get; set; }

        public HashSet<string> AppendUtf8CharsetOnContentTypes { get; set; }

        public Dictionary<string, TimeSpan> AddMaxAgeForStaticMimeTypes { get; set; }

        public List<Func<IHttpRequest, IHttpHandler>> RawHttpHandlers { get; set; }

        public bool OnlySendSessionCookiesSecurely { get; set; }
        public string RestrictAllCookiesToDomain { get; set; }

        public TimeSpan DefaultJsonpCacheExpiration { get; set; }
        public bool Return204NoContentForEmptyResponse { get; set; }
        public bool AllowPartialResponses { get; set; }

        public bool AllowNonHttpOnlyCookies { get; set; }

        private EndpointHostConfig()
        {
        }

        public void SetServiceStackHandlerFactoryPath(string serviceStackHandlerFactoryPath)
        {
            if (string.IsNullOrWhiteSpace(serviceStackHandlerFactoryPath))
                serviceStackHandlerFactoryPath = null;
            else
            {
                serviceStackHandlerFactoryPath = serviceStackHandlerFactoryPath.TrimStart('/').Trim().ToLower();
                if (string.IsNullOrWhiteSpace(serviceStackHandlerFactoryPath))
                    serviceStackHandlerFactoryPath = null;
            }

            ServiceStackHandlerFactoryPath = serviceStackHandlerFactoryPath;
            MetadataRedirectPath = ServiceStackHandlerFactoryPath == null ? "metadata" : PathUtils.CombinePaths(ServiceStackHandlerFactoryPath, "metadata");
        }

        public bool HasFeature(Feature feature)
        {
            return (feature & EnableFeatures) == feature;
        }

        public void AssertFeatures(Feature usesFeatures)
        {
            if (EnableFeatures == Feature.All) return;

            if (!HasFeature(usesFeatures))
            {
                throw new UnauthorizedAccessException(
                    String.Format("'{0}' Features have been disabled by your administrator", usesFeatures));
            }
        }

        public UnauthorizedAccessException UnauthorizedAccess(EndpointAttributes requestAttrs)
        {
            return new UnauthorizedAccessException(
                String.Format("Request with '{0}' is not allowed", requestAttrs));
        }

        public void AssertContentType(string contentType)
        {
            if (EnableFeatures == Feature.All) return;

            var contentTypeFeature = ContentType.ToFeature(contentType);
            AssertFeatures(contentTypeFeature);
        }

        public MetadataPagesConfig CreateMetadataPagesConfig(string servicePath)
        {
            return CreateMetadataPagesConfig(MetadataMap[servicePath]);
        }

        public MetadataPagesConfig CreateMetadataPagesConfig(ServiceMetadata metadata)
        {
            return new MetadataPagesConfig(
                metadata,
                ServiceEndpointsMetadataConfig,
                IgnoreFormatsInMetadata,
                EndpointHost.ContentTypeFilter.ContentTypeFormats.Keys.ToList());
        }

        public bool HasAccessToMetadata(IHttpRequest httpReq, IHttpResponse httpRes)
        {
            if (!HasFeature(Feature.Metadata))
            {
                HandleErrorResponse(httpReq, httpRes, HttpStatusCode.Forbidden, "Metadata Not Available");
                return false;
            }

            if (MetadataVisibility != EndpointAttributes.Any)
            {
                var actualAttributes = httpReq.GetAttributes();
                if ((actualAttributes & MetadataVisibility) != MetadataVisibility)
                {
                    HandleErrorResponse(httpReq, httpRes, HttpStatusCode.Forbidden, "Metadata Not Visible");
                    return false;
                }
            }
            return true;
        }

        public void HandleErrorResponse(IHttpRequest httpReq, IHttpResponse httpRes, HttpStatusCode errorStatus, string errorStatusDescription = null)
        {
            if (httpRes.IsClosed) return;

            httpRes.StatusDescription = errorStatusDescription;

            var handler = GetHandlerForErrorStatus(errorStatus, httpReq.ServicePath);

            ((IServiceStackHttpHandler)handler).ProcessRequest(httpReq, httpRes, httpReq.OperationName);
        }

        public IHttpHandler GetHandlerForErrorStatus(HttpStatusCode errorStatus, string servicePath)
        {
            switch (errorStatus)
            {
                case HttpStatusCode.Forbidden:
                    return new ForbiddenHttpHandler(servicePath);
                case HttpStatusCode.NotFound:
                    return new NotFoundHttpHandler(servicePath);
            }

            return new NotFoundHttpHandler(servicePath);
        }

        public void RegisterServiceException(Type exceptionType, string errorCode, LogLevel logLevel = LogLevel.ERROR)
        {
            if (exceptionType == null || string.IsNullOrWhiteSpace(errorCode))
                throw new ArgumentNullException("exceptionType & errorCode cannot be null or empty.");

            errorCode = errorCode.Trim();
            if (errorCode.StartsWith(ServiceUtils.ReservedErrorCodePrefix, StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException("Error Code cannot start with " + ServiceUtils.ReservedErrorCodePrefix
                    + " which is reserved for AntServiceStack framework.");

            ErrorUtils.ServiceExceptionErrorCodeMap[exceptionType] = new Tuple<string, LogLevel>(errorCode, logLevel);
        }

        public Action<object, IHttpRequest, IHttpResponse> PreExecuteServiceFilter { get; set; }

        public Action<object, IHttpRequest, IHttpResponse> PostExecuteServiceFilter { get; set; }

        public FallbackRestPathDelegate FallbackRestPath { get; set; }

        internal Dictionary<string, FallbackRestPathDelegate> FallbackRestPaths { get; set; }
    }

}
