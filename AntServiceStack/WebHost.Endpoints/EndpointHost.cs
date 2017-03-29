using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Funq;
using AntServiceStack.Common;
using AntServiceStack.Common.Configuration;
using AntServiceStack.Common.Web;
using AntServiceStack.Common.Extensions;
using AntServiceStack.Text;
using AntServiceStack.ServiceHost;
using AntServiceStack.ServiceModel.Serialization;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.WebHost.Endpoints.Formats;
using AntServiceStack.WebHost.Endpoints.Support;
using AntServiceStack.WebHost.Endpoints.Utils;
using AntServiceStack.WebHost.Endpoints.Metadata.Config;
using AntServiceStack.Plugins.ConfigInfo;
using AntServiceStack.Plugins.WhiteList;
using AntServiceStack.Plugins.RateLimiting;
using AntServiceStack.Plugins.CustomOperation;
using AntServiceStack.Plugins.OperationInfo;
using AntServiceStack.Plugins.CrossDomain;
using AntServiceStack.Plugins.RouteInfo;
using AntServiceStack.Plugins.DynamicPolicy;
using AntServiceStack.Plugins.SimpleAuth;
using AntServiceStack.Plugins.BlackList;
using AntServiceStack.Plugins.Baiji;
using AntServiceStack.Plugins.Consul;
using AntServiceStack.Plugins.RequestCounter;
using AntServiceStackSwagger;

namespace AntServiceStack.WebHost.Endpoints
{
    public class EndpointHost
    {
        public static IAppHost AppHost { get; internal set; }

        public static IContentTypeFilter ContentTypeFilter { get; set; }

        public static Dictionary<string, string> SupportedFormats { get; private set; }

        public static List<Action<IHttpRequest, IHttpResponse>> RawRequestFilters { get; private set; }

        public static List<Action<IHttpRequest, IHttpResponse, object>> RequestFilters { get; private set; }

        public static List<Action<IHttpRequest, IHttpResponse, object>> ResponseFilters { get; private set; }

        public static List<Action<PostResponseFilterArgs>> PostResponseFilters { get; private set; }

        public static HandleUncaughtExceptionDelegate ExceptionHandler { get; set; }

        public static List<HttpHandlerResolverDelegate> CatchAllHandlers { get; set; }

        private static bool pluginsLoaded = false;

        public static List<IPlugin> Plugins { get; set; }

        public static DateTime StartedAt { get; set; }

        public static DateTime ReadyAt { get; set; }

        private static void Reset()
        {
            ContentTypeFilter = HttpResponseFilter.Instance;
            SupportedFormats = new Dictionary<string, string>()
            {
                { ContentType.GetContentFormat(ContentType.Xml), ContentType.Xml },
                { ContentType.GetContentFormat(ContentType.Json), ContentType.Json },
                { ContentType.GetContentFormat(ContentType.Jsv), ContentType.Jsv },
            };
            RawRequestFilters = new List<Action<IHttpRequest, IHttpResponse>>();
            RequestFilters = new List<Action<IHttpRequest, IHttpResponse, object>>();
            ResponseFilters = new List<Action<IHttpRequest, IHttpResponse, object>>();
            PostResponseFilters = new List<Action<PostResponseFilterArgs>>();
            CatchAllHandlers = new List<HttpHandlerResolverDelegate>();
            Plugins = new List<IPlugin>();
        }

        // Pre user config
        public static void ConfigureHost(IAppHost appHost, ServiceManager serviceManager)
        {
            Reset();
            AppHost = appHost;

            Config.ServiceManager = serviceManager;
            Config.DebugMode = appHost.GetType().Assembly.IsDebugBuild();

            Plugins.Add(new BaijiJsonFormat());
            Plugins.Add(new HtmlFormat());
            Plugins.Add(new PredefinedRoutesFeature());
            Plugins.Add(new MetadataFeature());
            Plugins.Add(new ConfigInfoPlugin());
            Plugins.Add(new CustomOperationPlugin());
            Plugins.Add(new OperationInfoPlugin());
            Plugins.Add(new RouteInfoPlugin());

            if (Config.DebugMode)
            {
                Plugins.Add(new RequestInfoFeature());
            }

            // By default, enable hystrix info for real-time preformance monitoring & troubleshooting
            Plugins.Add(new HystrixInfoFeature());

            Plugins.Add(new CrossDomainPlugin());

            Plugins.Add(new IPWhiteListPlugin());
            Plugins.Add(new AppIdWhiteListPlugin());

            Plugins.Add(new IPBlackListPlugin());
            Plugins.Add(new AppIdBlackListPlugin());

            Plugins.Add(new IPRateLimitingPlugin());
            Plugins.Add(new AppIdRateLimitingPlugin());

            Plugins.Add(new OperationRateLimitingPlugin());
            Plugins.Add(new ServiceRateLimitingPlugin());
            Plugins.Add(new SimpleAuthPlugin());
            Plugins.Add(new AsyncRequestCounterPlugin());
        }

        //After configure called
        public static void AfterInit()
        {
            StartedAt = DateTime.UtcNow;

            if (Config.EnableFeatures != Feature.All)
            {
                if ((Feature.Xml & Config.EnableFeatures) != Feature.Xml)
                    Config.IgnoreFormatsInMetadata.Add("xml");
                if ((Feature.Json & Config.EnableFeatures) != Feature.Json)
                    Config.IgnoreFormatsInMetadata.Add("json");
                if ((Feature.Jsv & Config.EnableFeatures) != Feature.Jsv)
                    Config.IgnoreFormatsInMetadata.Add("jsv");
                if ((Feature.Csv & Config.EnableFeatures) != Feature.Csv)
                    Config.IgnoreFormatsInMetadata.Add("csv");
                if ((Feature.Html & Config.EnableFeatures) != Feature.Html)
                    Config.IgnoreFormatsInMetadata.Add("html");
            }

            if ((Feature.Html & Config.EnableFeatures) != Feature.Html)
                Plugins.RemoveAll(x => x is HtmlFormat);

            if ((Feature.PredefinedRoutes & Config.EnableFeatures) != Feature.PredefinedRoutes)
                Plugins.RemoveAll(x => x is PredefinedRoutesFeature);

            if ((Feature.Metadata & Config.EnableFeatures) != Feature.Metadata)
                Plugins.RemoveAll(x => x is MetadataFeature);

            if ((Feature.RequestInfo & Config.EnableFeatures) != Feature.RequestInfo)
                Plugins.RemoveAll(x => x is RequestInfoFeature);

            if ((Feature.HystrixInfo & Config.EnableFeatures) != Feature.HystrixInfo)
                Plugins.RemoveAll(x => x is HystrixInfoFeature);

            if ((Feature.ProtoBuf & Config.EnableFeatures) != Feature.ProtoBuf)
                Plugins.RemoveAll(x => x is IProtoBufPlugin); //external

            if ((Feature.MsgPack & Config.EnableFeatures) != Feature.MsgPack)
                Plugins.RemoveAll(x => x is IMsgPackPlugin);  //external

            if (ExceptionHandler == null)
            {
                // This is where all framework exceptions are centrally handled
                ExceptionHandler = (httpReq, httpRes, ex) =>
                {
                    //httpRes.WriteToResponse always calls .Close in its finally statement so 
                    //if there is a problem writing to response, by now it will be closed
                    if (!httpRes.IsClosed)
                    {
                        httpRes.WriteErrorToResponse(httpReq, httpReq.ResponseContentType, ex);
                    }
                };
            }

            if (!Plugins.Exists(p => p is DynamicPolicyPlugin))
                Plugins.Add(new DynamicPolicyPlugin());

            if (Config.UseConsulDiscovery && !Plugins.Exists(p => p is ConsulFeature))
                Plugins.Add(new ConsulFeature());

            if (!Plugins.Exists(p => p is SwaggerServicePlugin))
                Plugins.Add(new SwaggerServicePlugin());

            //这里先预留 留着获取动态配置用的
            if (AntFxConfigWebServiceUtils.Enabled)
            {
                HystrixCommandHelper.SyncGlobalSetting();
                AntFxConfigWebServiceUtils.SubscribeFxWebServiceConfigUpdateEvent(HystrixCommandHelper.SyncGlobalSetting);
            }

            ConfigurePlugins();

            AppHost.LoadPlugin(Plugins.ToArray());
            pluginsLoaded = true;

            AfterPluginsLoaded();

            foreach (KeyValuePair<string, string> item in ContentTypeFilter.ContentTypeFormats)
            {
                SupportedFormats[item.Key] = item.Value;
            }

            if (AppHost.AfterInitCallbacks !=null && AppHost.AfterInitCallbacks.Count > 0)
            {
                AppHost.AfterInitCallbacks.ForEach(t=>t.Invoke(AppHost));
            }

            ReadyAt = DateTime.UtcNow;
        }

        public static T TryResolve<T>()
        {
            return AppHost != null ? AppHost.TryResolve<T>() : default(T);
        }

        /// <summary>
        /// The AppHost.Container. Note: it is not thread safe to register dependencies after AppStart.
        /// </summary>
        public static Container Container
        {
            get
            {
                var aspHost = AppHost as AppHostBase;
                if (aspHost != null)
                    return aspHost.Container;
                var listenerHost = AppHost as HttpListenerBase;
                return listenerHost != null ? listenerHost.Container : new Container(); //testing may use alt AppHost
            }
        }

        private static void ConfigurePlugins()
        {
            //Some plugins need to initialize before other plugins are registered.

            foreach (var plugin in Plugins)
            {
                var preInitPlugin = plugin as IPreInitPlugin;
                if (preInitPlugin != null)
                {
                    preInitPlugin.Configure(AppHost);
                }
            }
        }

        private static void AfterPluginsLoaded()
        {
            Config.ServiceManager.AfterInit();
            ServiceManager = Config.ServiceManager; //reset operations
        }

        public static T GetPlugin<T>() where T : class, IPlugin
        {
            return Plugins.FirstOrDefault(x => x is T) as T;
        }

        public static void AddPlugin(params IPlugin[] plugins)
        {
            if (pluginsLoaded)
            {
                AppHost.LoadPlugin(plugins);
            }
            else
            {
                foreach (var plugin in plugins)
                {
                    Plugins.Add(plugin);
                }
            }
        }

        public static ServiceManager ServiceManager
        {
            get { return Config.ServiceManager; }
            set { Config.ServiceManager = value; }
        }

        public static EndpointHostConfig Config
        {
            get
            {
                return EndpointHostConfig.Instance;
            }
        }

        public static bool DebugMode
        {
            get { return Config != null && Config.DebugMode; }
        }

        public static Dictionary<string, ServiceMetadata> MetadataMap { get { return Config.MetadataMap; } }

        /// <summary>
        /// Applies the raw request filters. Returns whether or not the request has been handled 
        /// and no more processing should be done.
        /// </summary>
        /// <returns></returns>
        public static bool ApplyPreRequestFilters(IHttpRequest httpReq, IHttpResponse httpRes)
        {
            foreach (var requestFilter in RawRequestFilters)
            {
                requestFilter(httpReq, httpRes);
                if (httpRes.IsClosed) break;
            }

            return httpRes.IsClosed;
        }

        /// <summary>
        /// Applies the request filters. Returns whether or not the request has been handled 
        /// and no more processing should be done.
        /// </summary>
        /// <returns></returns>
        public static bool ApplyRequestFilters(IHttpRequest httpReq, IHttpResponse httpRes, object requestDto, string operationName)
        {
            httpReq.ThrowIfNull("httpReq");
            httpRes.ThrowIfNull("httpRes");

            //Exec all RequestFilter attributes with Priority < 0
            var attributes = FilterAttributeCache.GetRequestFilterAttributes(httpReq.ServicePath, operationName);
                
            var i = 0;
            for (; i < attributes.Length && attributes[i].Priority < 0; i++)
            {
                var attribute = attributes[i];
                ServiceManager.Container.AutoWire(attribute);
                attribute.RequestFilter(httpReq, httpRes, requestDto);
                if (AppHost != null) //tests
                    AppHost.Release(attribute);
                if (httpRes.IsClosed) return httpRes.IsClosed;
            }

            //Exec global filters
            foreach (var requestFilter in RequestFilters)
            {
                requestFilter(httpReq, httpRes, requestDto);
                if (httpRes.IsClosed) return httpRes.IsClosed;
            }

            //Exec remaining RequestFilter attributes with Priority >= 0
            for (; i < attributes.Length; i++)
            {
                var attribute = attributes[i];
                ServiceManager.Container.AutoWire(attribute);
                attribute.RequestFilter(httpReq, httpRes, requestDto);
                if (AppHost != null) //tests
                    AppHost.Release(attribute);
                if (httpRes.IsClosed) return httpRes.IsClosed;
            }

            return httpRes.IsClosed;
        }

        /// <summary>
        /// Applies the response filters. Returns whether or not the request has been handled 
        /// and no more processing should be done.
        /// </summary>
        /// <returns></returns>
        public static bool ApplyResponseFilters(IHttpRequest httpReq, IHttpResponse httpRes, object response, string operationName)
        {
            httpReq.ThrowIfNull("httpReq");
            httpRes.ThrowIfNull("httpRes");

            //Exec all RequestFilter attributes with Priority < 0
            var attributes = FilterAttributeCache.GetResponseFilterAttributes(httpReq.ServicePath, operationName);

            var i = 0;
            for (; i < attributes.Length && attributes[i].Priority < 0; i++)
            {
                var attribute = attributes[i];
                ServiceManager.Container.AutoWire(attribute);
                attribute.ResponseFilter(httpReq, httpRes, response);
                if (AppHost != null) //tests
                    AppHost.Release(attribute);
                if (httpRes.IsClosed) return httpRes.IsClosed;
            }

            //Exec global filters
            foreach (var responseFilter in ResponseFilters)
            {
                responseFilter(httpReq, httpRes, response);
                if (httpRes.IsClosed) return httpRes.IsClosed;
            }

            //Exec remaining ResponseFilter attributes with Priority >= 0
            for (; i < attributes.Length; i++)
            {
                var attribute = attributes[i];
                ServiceManager.Container.AutoWire(attribute);
                attribute.ResponseFilter(httpReq, httpRes, response);
                if (AppHost != null) //tests
                    AppHost.Release(attribute);
                if (httpRes.IsClosed) return httpRes.IsClosed;
            }

            return httpRes.IsClosed;
        }

        /// <summary>
        /// Applies the post response filters.
        /// </summary>
        public static void ApplyPostResponseFilters(PostResponseFilterArgs args)
        {
            PostResponseFilters.ForEach(responseFilter => responseFilter(args));
        }

        internal static object ExecuteService(object request, EndpointAttributes endpointAttributes, IHttpRequest httpReq, IHttpResponse httpRes)
        {
            if (String.IsNullOrEmpty(httpReq.OperationName))
            {
                throw new ArgumentNullException("No operation name was supplied");
            }

            return Config.ServiceController.Execute(httpReq.OperationName, request,
                new HttpRequestContext(httpReq, httpRes, request, endpointAttributes));
        }

        /// <summary>
        /// Call to signal the completion of a ServiceStack-handled Request
        /// </summary>
        internal static void CompleteRequest()
        {
            try
            {
                if (AppHost != null)
                {
                    AppHost.OnEndRequest();
                }
            }
            catch (Exception) { }
        }

        public static void Dispose()
        {
            AppHost = null;
        }
    }
}