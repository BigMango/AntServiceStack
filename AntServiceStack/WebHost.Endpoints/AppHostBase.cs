using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Timers;
using Funq;
using AntServiceStack.Common;
using AntServiceStack.Configuration;
using Freeway.Logging;
using AntServiceStack.Common.Hystrix;
using AntServiceStack.Common.Hystrix.CircuitBreaker;
using AntServiceStack.ServiceHost;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.Common.Utils;
using System.Threading.Tasks;
using AntServiceStack.Text;
using AntServiceStack.WebHost.Endpoints.Registry;

namespace AntServiceStack.WebHost.Endpoints
{
    /// <summary>
    /// Inherit from this class if you want to host your web services inside an
    /// ASP.NET application.
    /// </summary>
    public abstract class AppHostBase
        : IFunqlet, IDisposable, IAppHost, IHasContainer
    {
        private readonly ILog log = LogManager.GetLogger(typeof(AppHostBase));


        private const long DEFAULT_METRIC_SENDING_INTERVAL = 60 * 1000; // 1 minutes

        /// <summary>
        /// 单例模式
        /// </summary>
        public static AppHostBase Instance { get; protected set; }

        static AppHostBase()
        {
          
        }
        public virtual void UpdateConfig<TV>(Expression<Func<EndpointHostConfig, TV>> config, TV value)
        {
            var expression = (MemberExpression)config.Body;
            string fieldName = expression.Member.Name;
            if (string.IsNullOrEmpty(fieldName))
            {
                return;
            }
            var prop = Config.GetType().GetProperty(fieldName);
            if (prop == null) 
            {
                throw new NotSupportedException("can not update Config value by name:{0}".Fmt(fieldName));
            }
            prop.SetValue(Config, value, null);
        }
        /// <summary>
        /// 配置EndpointHost
        /// 根据传入的Assembly 解析
        /// </summary>
        /// <param name="assembliesWithServices"></param>
        protected AppHostBase(params Assembly[] assembliesWithServices)
        {
            EndpointHost.ConfigureHost(this, CreateServiceManager(assembliesWithServices));
            AfterInitCallbacks = new List<Action<IAppHost>>();
            OnDisposeCallbacks = new List<Action<IAppHost>>();
        }

        protected AppHostBase(params Type[] serviceTypes)
        {
            EndpointHost.ConfigureHost(this, CreateServiceManager(serviceTypes));
            AfterInitCallbacks = new List<Action<IAppHost>>();
            OnDisposeCallbacks = new List<Action<IAppHost>>();
        }
        protected AppHostBase(string serviceName, params Type[] serviceTypes)
        {
            EndpointHost.ConfigureHost(this, CreateServiceManager(serviceName, serviceTypes));
            AfterInitCallbacks = new List<Action<IAppHost>>();
            OnDisposeCallbacks = new List<Action<IAppHost>>();
        }

        protected virtual ServiceManager CreateServiceManager(string serviceName, params Assembly[] assembliesWithServices)
        {
            return new ServiceManager(serviceName,assembliesWithServices);
        }
        protected virtual ServiceManager CreateServiceManager(string serviceName, params Type[] serviceTypes)
        {
            return new ServiceManager(serviceName,serviceTypes);
        }
        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="assembliesWithServices"></param>
        /// <returns></returns>
        protected virtual ServiceManager CreateServiceManager(params Assembly[] assembliesWithServices)
        {
            return new ServiceManager(assembliesWithServices);
        }

        protected virtual ServiceManager CreateServiceManager(params Type[] serviceTypes)
        {
            return new ServiceManager(serviceTypes);
        }

        protected IServiceController ServiceController
        {
            get
            {
                return EndpointHost.Config.ServiceController;
            }
        }

        internal DateTime StartUpTime { get; private set; }

        public Container Container
        {
            get
            {
                return EndpointHost.Config.ServiceManager != null
                    ? EndpointHost.Config.ServiceManager.Container : null;
            }
        }

        public void Init()
        {
            try
            {
                log.Info("SOA 2.0 Service Initialization", "SOA 2.0 Service is initializing...",
                        new Dictionary<string, string> 
                { 
                    { "StartUp", "SOA2.0" },
                    { "ErrorCode", "FXD300049"}
                });

                if (Instance != null)
                {
                    throw new InvalidDataException("AppHostBase.Instance has already been set");
                }

                Instance = this;

                var serviceManager = EndpointHost.Config.ServiceManager;
                if (serviceManager != null)
                {
                    serviceManager.Init();//服务初始化 route controller action
                    Configure(EndpointHost.Config.ServiceManager.Container);
                }
                else
                {
                    Configure(null);
                }

                EndpointHost.AfterInit();

                // periodically send metrics
                var mTimer = new Timer();
                mTimer.Interval = DEFAULT_METRIC_SENDING_INTERVAL;
                mTimer.Enabled = true;
                mTimer.AutoReset = true;
                mTimer.Elapsed += new ElapsedEventHandler(sendMetrics);

                StartUpTime = DateTime.Now;

                //序列化日期
                JsConfig.UseStandardLongDateTime();

                log.Info("SOA 2.0 Service Initialization", "SOA 2.0 Service has been initialized.",
                    new Dictionary<string, string> 
                { 
                    { "StartUp", "SOA2.0" },
                    { "ErrorCode", "FXD300050"}
                });
            }
            catch (Exception ex)
            {
                log.Error("Error occurred while initializing SOA 2.0 Service", ex,
                        new Dictionary<string, string> 
                { 
                    { "StartUp", "SOA2.0" },
                    { "ErrorCode", "FXD300086"}
                });
                throw;
            }
        }

        // send metrics to backend central logging system
        void sendMetrics(object sender, ElapsedEventArgs arg)
        {
            try
            {
                if (EndpointHost.MetadataMap == null)
                    return;
                foreach (ServiceMetadata metadata in EndpointHost.MetadataMap.Values)
                {
                    try
                    {
                        if (metadata.ServiceName == ServiceMetadata.AnonymousServiceName)
                            continue;

                        var tagMap = new Dictionary<string, string>();
                        tagMap["webservice"] = (ServiceUtils.ConvertNamespaceToMetricPrefix(metadata.ServiceNamespace) + "." + metadata.ServiceName)
                                .ToLower().Replace("soa.ant.com.", string.Empty);
                        tagMap["frameworkversion"] = string.Format("SS-{0} CG-{1}", metadata.AntServiceStackVersion, metadata.AntCodeGenVersion);
                        var now = DateTime.Now;
                        Action<HystrixCommandMetrics, long, string, string> logEventDistribution = (m, c, tn, tv) =>
                        {
                            if (c <= 0)
                                return;

                            tagMap[tn] = tv;
                            //metricLog.log(m.MetricNameEventDistribution, c, tagMap, now);
                        };
                        Action<HystrixCommandMetrics, long, long?, string, string> logLatencyDistribution = (m, s, e, tn, tv) =>
                        {
                            int count = m.GetServiceExecutionCountInTotalTimeRange(s, e);
                            if (count <= 0)
                                return;

                            tagMap[tn] = tv;
                           // metricLog.log(m.MetricNameLatencyDistribution, count, tagMap, now);
                        };
                        Action<HystrixCommandMetrics, double, string, string> logLatencyPencentile = (m, p, tn, tv) =>
                        {
                            long pencentile = m.GetTotalTimePercentile(p);
                            if (pencentile <= 0)
                                return;

                            tagMap[tn] = tv;
                           // metricLog.log(m.MetricNameLatencyPercentile, pencentile, tagMap, now);
                        };

                        foreach (Operation operation in metadata.Operations)
                        {
                            HystrixCommandMetrics commandMetrics = operation.HystrixCommand.Metrics;
                            tagMap["operation"] = tagMap["webservice"] + "." + commandMetrics.OperationName.ToLower();

                            long successCount = commandMetrics.GetSuccessCount();
                            long frameworkErrorCount = commandMetrics.GetFrameworkErrorCount();
                            long serviceErrorCount = commandMetrics.GetServiceErrorCount();
                            long validationErrorCount = commandMetrics.GetValidationErrorCount();
                            long shortCircuitedCount = commandMetrics.GetShortCircuitCount();
                            long timeoutCount = commandMetrics.GetTimeoutCount();
                            long threadPoolRejectedCount = commandMetrics.GetThreadPoolRejectedCount();
                            commandMetrics.ResetMetricsCounters();

                            long totalCount = successCount + frameworkErrorCount + serviceErrorCount + validationErrorCount + shortCircuitedCount + timeoutCount + threadPoolRejectedCount;
                            //metricLog.log(commandMetrics.MetricNameRequestCount, totalCount, tagMap, now);

                           // if(operation.IsAsync)
                                //metricLog.log(commandMetrics.MetricNameConcurrentExecutionCount, commandMetrics.CurrentConcurrentExecutionCount, tagMap, now);

                            var tagName = "distribution";
                            logEventDistribution(commandMetrics, successCount, tagName, "Success");
                            logEventDistribution(commandMetrics, shortCircuitedCount, tagName, "Short Circuited");
                            logEventDistribution(commandMetrics, timeoutCount, tagName, "Timeout");
                            logEventDistribution(commandMetrics, threadPoolRejectedCount, tagName, "Threadpool Rejected");
                            logEventDistribution(commandMetrics, frameworkErrorCount, tagName, "Framework Exception");
                            logEventDistribution(commandMetrics, serviceErrorCount, tagName, "Service Exception");
                            logEventDistribution(commandMetrics, validationErrorCount, tagName, "Validation Exception");
                            if (tagMap.ContainsKey(tagName))
                                tagMap.Remove(tagName);

                            int count;
                            long sum, min, max;
                            commandMetrics.GetTotalTimeMetricsData(out count, out sum, out min, out max);
                            tagName = "SetFeatureType";
                            tagMap[tagName] = "count";
                           // metricLog.log(commandMetrics.MetricNameLatency, count, tagMap, now);
                            tagMap[tagName] = "sum";
                            //metricLog.log(commandMetrics.MetricNameLatency, sum, tagMap, now);
                            tagMap[tagName] = "min";
                           // metricLog.log(commandMetrics.MetricNameLatency, min, tagMap, now);
                           // tagMap[tagName] = "max";
                            //metricLog.log(commandMetrics.MetricNameLatency, max, tagMap, now);
                            tagMap.Remove(tagName);

                            tagName = "distribution";
                            logLatencyDistribution(commandMetrics, 0, 10, tagName, "0 ~ 10ms");
                            logLatencyDistribution(commandMetrics, 10, 50, tagName, "10 ~ 50ms");
                            logLatencyDistribution(commandMetrics, 50, 200, tagName, "50 ~ 200ms");
                            logLatencyDistribution(commandMetrics, 200, 500, tagName, "200 ~ 500ms");
                            logLatencyDistribution(commandMetrics, 500, 1000, tagName, "500ms ~ 1s");
                            logLatencyDistribution(commandMetrics, 1000, 5 * 1000, tagName, "1 ~ 5s");
                            logLatencyDistribution(commandMetrics, 5 * 1000, 10 * 1000, tagName, "5 ~ 10s");
                            logLatencyDistribution(commandMetrics, 10 * 1000, 30 * 1000, tagName, "10 ~ 30s");
                            logLatencyDistribution(commandMetrics, 30 * 1000, 100 * 1000, tagName, "30 ~ 100s");
                            logLatencyDistribution(commandMetrics, 100 * 1000, null, tagName, ">= 100s");
                            if (tagMap.ContainsKey(tagName))
                                tagMap.Remove(tagName);

                            tagName = "percentile";
                            logLatencyPencentile(commandMetrics, 0, tagName, "0");
                            logLatencyPencentile(commandMetrics, 25, tagName, "25");
                            logLatencyPencentile(commandMetrics, 50, tagName, "50");
                            logLatencyPencentile(commandMetrics, 75, tagName, "75");
                            logLatencyPencentile(commandMetrics, 90, tagName, "90");
                            logLatencyPencentile(commandMetrics, 95, tagName, "95");
                            logLatencyPencentile(commandMetrics, 99, tagName, "99");
                            logLatencyPencentile(commandMetrics, 99.5, tagName, "99.5");
                            logLatencyPencentile(commandMetrics, 100, tagName, "100");
                            if (tagMap.ContainsKey(tagName))
                                tagMap.Remove(tagName);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Fail to send metrics to backend!", ex, new Dictionary<string, string>() { { "ErrorCode", "FXD300009" } });
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Fail to send metrics to backend!", ex, new Dictionary<string, string>() { { "ErrorCode", "FXD300009" } });
            }
        }

        public abstract void Configure(Container container);

        public void RegisterAs<T, TAs>() where T : TAs
        {
            this.Container.RegisterAutoWiredAs<T, TAs>();
        }

        public virtual void Release(object instance)
        {
            try
            {
                var iocAdapterReleases = Container.Adapter as IRelease;
                if (iocAdapterReleases != null)
                {
                    iocAdapterReleases.Release(instance);
                }
                else
                {
                    var disposable = instance as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
            catch {/*ignore*/}
        }

        public virtual void OnEndRequest()
        {
            foreach (var item in HostContext.Instance.Items.Values)
            {
                Release(item);
            }

            HostContext.Instance.EndRequest();
        }

        public void Register<T>(T instance)
        {
            this.Container.Register(instance);
        }

        public T TryResolve<T>()
        {
            return this.Container.TryResolve<T>();
        }

        /// <summary>
        /// Resolves from IoC container a specified type instance.
        /// </summary>
        /// <typeparam name="T">Type to be resolved.</typeparam>
        /// <returns>Instance of <typeparamref name="T"/>.</returns>
        public static T Resolve<T>()
        {
            if (Instance == null) throw new InvalidOperationException("AppHostBase is not initialized.");
            return Instance.Container.Resolve<T>();
        }

        /// <summary>
        /// Resolves and auto-wires a ServiceStack Service
        /// </summary>
        /// <typeparam name="T">Type to be resolved.</typeparam>
        /// <returns>Instance of <typeparamref name="T"/>.</returns>
        public static T ResolveService<T>(HttpContext httpCtx) where T : class, IRequiresRequestContext
        {
            if (Instance == null) throw new InvalidOperationException("AppHostBase is not initialized.");
            var service = Instance.Container.Resolve<T>();
            if (service == null) return null;
            service.RequestContext = httpCtx.ToRequestContext();
            return service;
        }

        public Dictionary<Type, Func<IHttpRequest, object>> RequestBinders
        {
            get { return EndpointHost.ServiceManager.ServiceController.RequestTypeFactoryMap; }
        }

        public IContentTypeFilter ContentTypeFilters
        {
            get
            {
                return EndpointHost.ContentTypeFilter;
            }
        }

        public List<Action<IHttpRequest, IHttpResponse>> PreRequestFilters
        {
            get
            {
                return EndpointHost.RawRequestFilters;
            }
        }

        public List<Action<IHttpRequest, IHttpResponse, object>> RequestFilters
        {
            get
            {
                return EndpointHost.RequestFilters;
            }
        }

        public List<Action<IHttpRequest, IHttpResponse, object>> ResponseFilters
        {
            get
            {
                return EndpointHost.ResponseFilters;
            }
        }

        public List<Action<PostResponseFilterArgs>> PostResponseFilters
        {
            get
            {
                return EndpointHost.PostResponseFilters;
            }
        }

        public HandleUncaughtExceptionDelegate ExceptionHandler
        {
            get { return EndpointHost.ExceptionHandler; }
            set { EndpointHost.ExceptionHandler = value; }
        }

        public List<HttpHandlerResolverDelegate> CatchAllHandlers
        {
            get { return EndpointHost.CatchAllHandlers; }
        }

        public EndpointHostConfig Config
        {
            get { return EndpointHost.Config; }
        }

        public List<IPlugin> Plugins
        {
            get { return EndpointHost.Plugins; }
        }

        public virtual string ResolveAbsoluteUrl(string virtualPath, IHttpRequest httpReq)
        {
            return Config.WebHostUrl == null
                ? VirtualPathUtility.ToAbsolute(virtualPath)
                : httpReq.GetAbsoluteUrl(virtualPath);
        }

        public List<Action<IAppHost>> AfterInitCallbacks { get; }
        public List<Action<IAppHost>> OnDisposeCallbacks { get; }

        public virtual void LoadPlugin(params IPlugin[] plugins)
        {
            foreach (var plugin in plugins)
            {
                try
                {
                    plugin.Register(this);
                }
                catch (Exception ex)
                {
                    log.Fatal("Error loading plugin " + plugin.GetType().Name, ex, new Dictionary<string, string> { { "ErrorCode", "FXD300007" } });
                }
            }
        }

        public virtual object ExecuteService(string operationName, object requestDto)
        {
            return ExecuteService(operationName, requestDto, EndpointAttributes.None);
        }

        public object ExecuteService(string operationName, object requestDto, EndpointAttributes endpointAttributes)
        {
            return EndpointHost.Config.ServiceController.Execute(operationName, requestDto,
                new HttpRequestContext(requestDto, endpointAttributes));
        }

        public void RegisterService(Type serviceType)
        {
            EndpointHost.Config.ServiceManager.RegisterService(serviceType);
        }

        public virtual void Dispose()
        {
            if (OnDisposeCallbacks != null)
            {
                foreach (var callback in OnDisposeCallbacks)
                {
                    callback(this);
                }
            }

            if (EndpointHost.Config.ServiceManager != null)
            {
                EndpointHost.Config.ServiceManager.Dispose();
            }
        }
    }
}
