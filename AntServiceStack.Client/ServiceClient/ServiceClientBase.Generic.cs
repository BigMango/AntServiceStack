using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Concurrent;
using System.Linq;
using Freeway.Logging;

using AntServiceStack.Common.Utils;
using AntServiceStack.Common.Configuration;
using ExecutionContext = AntServiceStack.Common.Execution.ExecutionContext;
using AntServiceStack.Client.CHystrix;
using AntServiceStack.Client.RegistryClient;
using AntServiceStack.Client.ServiceClient;
using AntServiceStack.Common.Config;
using AntServiceStack.Common.ServiceClient;

namespace AntServiceStack.ServiceClient
{
    public abstract partial class ServiceClientBase<DerivedClient> : ServiceClientBase
        where DerivedClient : ServiceClientBase<DerivedClient> 
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ServiceClientBase<DerivedClient>));

        private static readonly string clientTypeName = typeof(DerivedClient).Name;
        


        /// <summary>
        /// 获取客户实例单件，直连模式，用于开发测试。
        /// 
        /// 在开发环境中，可以使用该方法获取对应目标服务的client实例，该client会以单件(singleton)形式被缓存。
        /// 
        /// 注意，请不要在生产或者正式的测试环境中使用该方法获取client实例，因为该方法获取的client实例直接和目标
        /// 服务地址绑定，在服务目标地址发生变化时，该client实例将可能无法正确访问服务,另外，该客户实例也不会将性能
        /// 统计数据(metric)发送到后端(Central Logging)。
        /// </summary>
        /// <param name="baseUrl">target service url</param>
        /// <returns>singleton client instance</returns>
        /// 
        public static DerivedClient GetInstance(string baseUri)
        {
            if (string.IsNullOrWhiteSpace(baseUri))
            {
                var errorMsg = "Missing mandatory baseUri param.";
                log.Fatal(errorMsg, new Dictionary<string, string>() { { "ErrorCode", "FXD301000" } });
                throw new ArgumentNullException(errorMsg);
            }
            baseUri = baseUri.Trim();

            Type[] paramTypes = new Type[] { typeof(string) };
            object[] paramValues = new object[] { baseUri };
            return GetInstance(paramTypes, paramValues);
        }

        /// <summary>
        /// 获取客户实例单件，间接连接模式。
        /// 
        /// 在生产环境中或正式的测试环境中，请使用该方法获取对应目标服务的client实例，该client会以单件(singleton)形式被缓存
        /// 并会周期性地查询服务注册表以更新可能变化的目标服务地址。
        /// 
        /// 对于测试环境(非生产和UAT环境)，需要在配置文件appSettings里配置SOA.ServiceRegistry.TestSubEnv，提供子环境名，如fws, fat, lpt，否则无法正确查找目标服务器地址。
        /// 配置如：&lt;add key="SOA.ServiceRegistry.TestSubEnv" value="fws"/&gt;。
        /// 生产和UAT环境目前暂无子环境要求。
        /// </summary>
        /// <returns>singleton service client</returns>
        public static DerivedClient GetInstance()
        {
            Type[] paramTypes = new Type[] { typeof(string), typeof(string), typeof(string) };
            object[] paramValues = new object[] { null, null, null };
            return GetInstance(paramTypes, paramValues);
        }

        internal static DerivedClient GetInstance(Type[] paramTypes, object[] paramValues)
        {
            Type clientType = typeof(DerivedClient);
            if (!clientCache.ContainsKey(clientType.FullName))
            {
                lock(clientCache)
                {
                    if (!clientCache.ContainsKey(clientType.FullName))
                    {
                        ConstructorInfo ci = clientType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, paramTypes, null);
                        clientCache[clientType.FullName] = (DerivedClient)ci.Invoke(paramValues);
                    }
                }
            }

            return (DerivedClient)clientCache[clientType.FullName];
        }

        /// <summary>
        /// Whether to Accept Gzip,Deflate Content-Encoding and to auto decompress responses
        /// </summary>
        public bool DisableAutoCompression { get; set; }

        public bool DeserializeResponseUseMemoryStream { get; set; }

        /// <summary>
        /// Gets the collection of headers to be added to outgoing requests.
        /// </summary>
        public NameValueCollection Headers { get; private set; }

        /// <summary>
        /// 代理设置
        /// </summary>
        public IWebProxy Proxy { get; set; }

        // sub-environment, such as fws, ltp, fat for test
        internal readonly string TestSubEnv;

        private readonly ConnectionMode ConnectionMode;

        private readonly bool LogErrorWithRequestInfo;

        private readonly bool LogWebExceptionAsError;

        private readonly bool LogCServiceExceptionAsError;

        private readonly bool CatIgnoreWarningWebException;

        private readonly bool HandleServiceErrorManually;

        private bool _enableCHystrixSupport;

        private ILoadBalancerRequestContext requestContext;

        /// <summary>
        /// 默认true
        /// </summary>
        public bool EnableCHystrixSupport
        {
            get { return _enableCHystrixSupport; }
            set { _enableCHystrixSupport = value && CHystrixIntegration.HasCHystrix; }
        }

        private bool _enableCHystrixSupportForIOCPAsync;
        /// <summary>
        /// 默认true
        /// 异步调用存在2种使用场景：
        /// 1. 非阻塞调用，不关心调用结果，只在callback里处理可能的异常，不确定是否应该启用熔断
        ///2. 阻塞调用，关心调用结果，类似于同步调用，可以启用熔断
        ///由于2种场景对熔断的需要不同，所以不能统一设置是否启用熔断。默认对IOCP调用不启用熔断
        /// </summary>
        public bool EnableCHystrixSupportForIOCPAsync
        {
            get { return _enableCHystrixSupportForIOCPAsync; }
            set { _enableCHystrixSupportForIOCPAsync = value && EnableCHystrixSupport && CHystrixIntegration.HasCHystrixIsolationUtils; }
        }

        private bool _enableTimeoutForIOCPAsync;
        internal bool EnableTimeoutForIOCPAsync
        {
            get { return _enableTimeoutForIOCPAsync; }
            set { _enableTimeoutForIOCPAsync = value; }
        }

        private int _chystrixCommandMaxConcurrentCount;
        public int CHystrixCommandMaxConcurrentCount
        {
            get { return _chystrixCommandMaxConcurrentCount; }
            set
            {
                if (value > 0)
                    _chystrixCommandMaxConcurrentCount = value;
            }
        }

        private readonly ConcurrentDictionary<string, int> CHystrixCommandMaxConcurrentCountMap;

        private readonly string HystrixCommandGroupKey;
        private readonly string HystrixCommandKeyPrefixForIOCPAsync;
        private readonly string HystrixCommandDomain;
        private readonly ConcurrentDictionary<string, string> CHystrixCommandKeys;
        private readonly ConcurrentDictionary<string, string> CHystrixIOCPCommandKeys;
        private readonly ConcurrentDictionary<string, string> CHystrixUrlInstanceKeyCache;
        private readonly ConcurrentDictionary<string, string> OperationKeys;

        protected string CCodeGeneratorVersion { get; private set; }

        private ThreadLocal<string> _threadLocalBaseUri = new ThreadLocal<string>();

        private string _baseUri;

        /// <summary>
        /// 从consul获取服务列表 最大失败重试10次
        /// </summary>
        private int initUrlRetryTimesProperty = 10;
        /// <summary>
        /// 目标服务base uri
        /// </summary>
        public string BaseUri 
        {
            get 
            {
                if (ConnectionMode == ConnectionMode.Direct)
                    return _baseUri;

                if (_threadLocalBaseUri.IsValueCreated)
                    return _threadLocalBaseUri.Value;

               // return _threadLocalBaseUri.Value = _baseUri;
                return _threadLocalBaseUri.Value = requestContext == null ? null : GetServiceUrl(requestContext);
            }
            protected internal set
            {
                if (ConnectionMode == ConnectionMode.Direct)
                {
                    _baseUri = value;
                    requestContext = new StaticRequestContext(this.ServiceFullName,value);
                    return;
                }

                _threadLocalBaseUri.Value = value;
            }
        }

        /// <summary>
        /// 目标服务名称，可从服务库(Service Repositroy)中查询
        /// </summary>
        public string ServiceName { get; private set; }

        /// <summary>
        /// 目标服务名字空间，可从服务库(Service Repositroy)中查询
        /// </summary>
        public string ServiceNamespace { get; private set; }

        internal string ServiceType { get; private set; }

        internal bool IsSLBService { get; private set; }

        internal string ServiceFullName { get; private set; }

        public string ServiceContact { get; internal set; }

        private string _format;
        /// <summary>
        /// 设定和获取当前的调用格式
        /// </summary>
        public string Format
        {
            get
            {
                return _format;
            }
            set
            {
                var formatValue = value == null ? value : value.ToLower();
                if (!CallFormats.ContainsKey(formatValue))
                    throw new ArgumentException(string.Format("Format {0} is not supported.", value));

                if (IsSLBService)
                {
                    if (!IsValidJavaCallFormat(formatValue))
                        throw new ArgumentException(string.Format("Format {0} is not supported.", value));
                }

                _format = formatValue;
            }
        }

        private IClientCallFormat CurrentCallFormat
        {
            get
            {
                return CallFormats[Format];
            }
        }

        public string Accept
        {
            get { return ContentType; }
        }

        public string ContentType
        { 
            get
            {
                IClientCallFormat callFormat = CallFormats[this.Format];
                return callFormat != null ? callFormat.ContentType : null;
            } 
        }

        public string HttpMethod { get { return DefaultHttpMethod; } }

        public bool AllowAutoRedirect { get; set; }

        /// <summary>
        /// The request filter is called before any request.
        /// This request filter only works with the instance where it was set (not global).
        /// </summary>
        public Action<HttpWebRequest> LocalHttpWebRequestFilter { get; set; }

        /// <summary>
        /// The response action is called once the server response is available.
        /// It will allow you to access raw response information. 
        /// Note that you should NOT consume the response stream as this is handled by ServiceStack
        /// </summary>
        public Action<HttpWebResponse> LocalHttpWebResponseFilter { get; set; }

        public Action<string, string, string, object> LocalRequestFilter { get; set; }

        public Action<string, string, string, object> LocalResponseFilter { get; set; }

        public Action<ExecutionContext> LocalRequestEndFilter { get; set; }


        private ServiceClientBase(ConnectionMode connectionMode)
        {
            ConnectionMode = connectionMode;

            AllowAutoRedirect = true;
            Headers = new NameValueCollection();

            //加载Client cs里面的 字段值
            InitMetadata();

            InitializeCallFormat(ObjectFactory.CreateAppSettingConfiguration());

            InitOperationTimeoutSettings(ServiceName, ServiceNamespace);

            string settingKey = GetServiceSettingKey(LogErrorWithRequestInfoSettingKey, ServiceName, ServiceNamespace);
            if (!bool.TryParse(ConfigUtils.GetNullableAppSetting(settingKey), out LogErrorWithRequestInfo))
                LogErrorWithRequestInfo = DefaultLogErrorWithRequestInfo;

            settingKey = GetServiceSettingKey(LogWebExceptionAsErrorSettingKey, ServiceName, ServiceNamespace);
            if (!bool.TryParse(ConfigUtils.GetNullableAppSetting(settingKey), out LogWebExceptionAsError))
                LogWebExceptionAsError = DefaultLogWebExceptionAsError;

            settingKey = GetServiceSettingKey(LogCServiceExceptionAsErrorSettingKey, ServiceName, ServiceNamespace);
            if (!bool.TryParse(ConfigUtils.GetNullableAppSetting(settingKey), out LogCServiceExceptionAsError))
                LogCServiceExceptionAsError = DefaultLogCServiceExceptionAsError;

            settingKey = GetServiceSettingKey(CatIgnoreWarningWebExceptionSettingKey, ServiceName, ServiceNamespace);
            if (!bool.TryParse(ConfigurationManager.AppSettings[settingKey], out CatIgnoreWarningWebException))
                CatIgnoreWarningWebException = DefaultCatIgnoreWarningWebException;

            settingKey = GetServiceSettingKey(HandleServiceErrorManuallySettingKey, ServiceName, ServiceNamespace);
            if (!bool.TryParse(ConfigUtils.GetNullableAppSetting(settingKey), out HandleServiceErrorManually))
                HandleServiceErrorManually = DefaultHandleServiceErrorManually;

            #region Hystrix配置
            //SOA.EnableCHystrixSupport
            settingKey = GetServiceSettingKey(EnableCHystrixSupportSettingKey, ServiceName, ServiceNamespace);
            bool enableCHystrixSupport;
            if (!bool.TryParse(ConfigUtils.GetNullableAppSetting(settingKey), out enableCHystrixSupport))
                enableCHystrixSupport = DefaultEnableCHystrixSupport;
            EnableCHystrixSupport = enableCHystrixSupport;

            //SOA.EnableCHystrixSupportForIOCPAsync
            settingKey = GetServiceSettingKey(EnableCHystrixSupportForIOCPAsyncSettingKey, ServiceName, ServiceNamespace);
            bool enableCHystrixSupportForIOCPAsync;
            if (!bool.TryParse(ConfigUtils.GetNullableAppSetting(settingKey), out enableCHystrixSupportForIOCPAsync))
                enableCHystrixSupportForIOCPAsync = DefaultEnableCHystrixSupportForIOCPAsync;
            EnableCHystrixSupportForIOCPAsync = enableCHystrixSupportForIOCPAsync;

            //SOA.EnableTimeoutForIOCPAsync
            settingKey = GetServiceSettingKey(EnableTimeoutForIOCPAsyncSettingKey, ServiceName, ServiceNamespace);
            if (!bool.TryParse(ConfigurationManager.AppSettings[settingKey], out _enableTimeoutForIOCPAsync))
                _enableTimeoutForIOCPAsync = DefaultEnableTimeoutForIOCPAsync;

            //SOA.CHystrixCommandMaxConcurrentCount
            settingKey = GetServiceSettingKey(CHystrixCommandMaxConcurrentCountSettingKey, ServiceName, ServiceNamespace);
            string settingValue = ConfigUtils.GetNullableAppSetting(settingKey);
            if (!int.TryParse(settingValue, out _chystrixCommandMaxConcurrentCount) || _chystrixCommandMaxConcurrentCount < 0)
                _chystrixCommandMaxConcurrentCount = 0;
            CHystrixCommandMaxConcurrentCountMap = new ConcurrentDictionary<string, int>();
            Dictionary<string, string> maxConcurrentCountMap = ConfigUtils.GetDictionaryFromAppSettingValue(settingValue);
            foreach (KeyValuePair<string, string> pair in maxConcurrentCountMap)
            {
                int count;
                int.TryParse(pair.Value, out count);
                if (count > 0)
                    CHystrixCommandMaxConcurrentCountMap[pair.Key.ToLower()] = count;
            }

            HystrixCommandDomain = ServiceUtils.GetServiceDomain(ServiceFullName);
            HystrixCommandGroupKey = CHystrixCommandKeyPrefixForSync + ServiceFullName;
            HystrixCommandKeyPrefixForIOCPAsync = CHystrixCommandKeyPrefixForIOCPAsync + ServiceFullName;
            CHystrixCommandKeys = new ConcurrentDictionary<string, string>();
            CHystrixIOCPCommandKeys = new ConcurrentDictionary<string, string>();
            OperationKeys = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            CHystrixUrlInstanceKeyCache = new ConcurrentDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            #endregion


            bool disableAutoCompression;
            settingKey = GetServiceSettingKey(DisableAutoCompressionSettingKey, ServiceName, ServiceNamespace);
            if (!bool.TryParse(ConfigUtils.GetNullableAppSetting(settingKey), out disableAutoCompression))
                disableAutoCompression = DefaultDisableAutoCompression;
            DisableAutoCompression = disableAutoCompression;

            string deserializeResponseUseMemoryStreamSettingKey = GetServiceSettingKey(DeserializeResponseUseMemoryStreamSettingKey, ServiceName, ServiceNamespace);
            bool deserializeResponseUseMemoryStream;
            if (!bool.TryParse(ConfigUtils.GetNullableAppSetting(deserializeResponseUseMemoryStreamSettingKey), out deserializeResponseUseMemoryStream))
                deserializeResponseUseMemoryStream = DefaultDeserializeResponseUseMemoryStream;
            this.DeserializeResponseUseMemoryStream = deserializeResponseUseMemoryStream;

    

           
        }

        protected ServiceClientBase(string baseUri)
            : this(ConnectionMode.Direct)
        {
            BaseUri = baseUri;
            log.Info(string.Format("Initialized client instance with direct service url {0}", baseUri), GetClientInfo().AddErrorCode("FXD301029"));
        }

        // serviceName和serviceNamespace这2个parameter已经过时，只作为placeholder存在，值没有意义
        protected ServiceClientBase(string serviceName, string serviceNamespace, string subEnv) 
            : this(ConnectionMode.Indirect)
        {
            
            //string settingKey = GetServiceSettingKey(SERVICE_REGISTRY_SUBENV_KEY, ServiceName, ServiceNamespace);
            //TestSubEnv = ConfigUtils.GetNullableAppSetting(settingKey);
            //if (string.IsNullOrWhiteSpace(TestSubEnv))
            //    TestSubEnv = ServiceRegistryTestSubEnv;
            //else
            //    TestSubEnv = TestSubEnv.Trim().ToLower();

            int count = -1;
          
            requestContext= DynamicRequestContextProvider.LoadSignalRRequestContext(ServiceFullName);
            while (count++ < initUrlRetryTimesProperty && (requestContext == null || requestContext.Server == null))
            {
                log.Info("Service url is null or empty, will retry after 100 ms", GetClientInfo());
                Thread.Sleep(100);
                requestContext = DynamicRequestContextProvider.LoadSignalRRequestContext(ServiceFullName, TestSubEnv);
            }
            if (count >= initUrlRetryTimesProperty)
            {
                log.Error(string.Format("Service is null after retried {0} times", count), GetClientInfo());
            }
            else
            {
                string serviceUrl = GetServiceUrl(requestContext);
                if (string.IsNullOrWhiteSpace(serviceUrl))
                {
                    log.Error("Got null or empty service url.", GetClientInfo());
                }
                else
                {
                    _threadLocalBaseUri.Value = serviceUrl;
                    string message;
                    if (count > 0)
                        message = string.Format("Got service url {0} after retried {1} times", serviceUrl, count);
                    else
                        message = string.Format("Initialized client instance with indirect service url {0}", serviceUrl);
                    log.Info(message, GetClientInfo());
                }
            }
        }

        private string GetServiceUrl(ILoadBalancerRequestContext requestContext)
        {
            if (requestContext == null)
                return null;

            var server = requestContext.Server;
            if (server == null)
                return null;

            if (!server.Any(r=>r.ServiceName.Equals(this.ServiceFullName)))
            {
                Dictionary<string, string> addtionalInfo = GetClientInfo();
                log.Error(ArtemisConstants.MetadataIsNullOrUrlNotExisted, server.ToString(), addtionalInfo);
                return null;
            }
            if (server.Length == 1)
            {
                return server.First().ServiceAddress;
            }
            return server[ThreadLocalRandom.Current.Next(0, server.Length)].ServiceAddress; 
        }

        private int GetCHystrixCommandMaxConcurrentCount(string operation)
        {
            if (string.IsNullOrWhiteSpace(operation))
                return 0;
            operation = operation.ToLower();

            int count;
            CHystrixCommandMaxConcurrentCountMap.TryGetValue(operation, out count);
            if (count > 0)
                return count;

            if (CHystrixCommandMaxConcurrentCount > 0)
                return CHystrixCommandMaxConcurrentCount;

            DefaultCHystrixCommandMaxConcurrentCountMap.TryGetValue(operation, out count);
            if (count > 0)
                return count;

            return DefaultCHystrixCommandMaxConcurrentCount;
        }

        public void ConfigCHystrixCommandMaxConcurrentCount(string operation, int maxConcurrentCount)
        {
            if (maxConcurrentCount <= 0)
                return;
            CHystrixCommandMaxConcurrentCountMap[operation.ToLower()] = maxConcurrentCount;
        }

        private void GetCHystrixCommandKey(ClientExecutionContext context, out string chystrixCommandKey, out string chystrixInstanceKey)
        {
            string instanceKey = context.Host;
            chystrixCommandKey = CHystrixCommandKeys.GetOrAdd(
                CHystrixCommandKeyPrefixForSync + context.Operation + "/" + instanceKey,
                key =>
                {
                    string commandKey = HystrixCommandGroupKey + "." + context.Operation.ToLower();
                    int count = GetCHystrixCommandMaxConcurrentCount(context.Operation);
                    if (count > 0)
                        CHystrixIntegration.ConfigCommand(instanceKey, commandKey, HystrixCommandGroupKey, HystrixCommandDomain, count);
                    else
                        CHystrixIntegration.ConfigCommand(instanceKey, commandKey, HystrixCommandGroupKey, HystrixCommandDomain);

                    return commandKey;
                });
            chystrixInstanceKey = instanceKey;
        }

        private void GetCHystrixCommandKeyForIOCPAsync(ClientExecutionContext context, out string chystrixCommandKey, out string chystrixInstanceKey)
        {
            string instanceKey = context.Host;
            chystrixCommandKey = CHystrixIOCPCommandKeys.GetOrAdd(
                CHystrixCommandKeyPrefixForIOCPAsync + context.Operation + "/" + instanceKey,
                key =>
                {
                    string commandKey = HystrixCommandKeyPrefixForIOCPAsync + "." + context.Operation.ToLower();
                    int count = GetCHystrixCommandMaxConcurrentCount(context.Operation);
                    if (count > 0)
                        CHystrixIntegration.UtilsSemaphoreIsolationConfig(instanceKey, commandKey, HystrixCommandGroupKey, HystrixCommandDomain, count);
                    else
                        CHystrixIntegration.UtilsSemaphoreIsolationConfig(instanceKey, commandKey, HystrixCommandGroupKey, HystrixCommandDomain);

                    return commandKey;
                });
            chystrixInstanceKey = instanceKey;
        }

        protected internal Dictionary<string, string> GetClientInfo()
        {
            Dictionary<string, string> clientInfo = new Dictionary<string, string>()
            {
                { "Version", AntServiceStackVersion },
                { "Service", ServiceName + "{" + ServiceNamespace + "}" },
                { "ServiceContact", ServiceContact },
                { "ConnectionMode", ConnectionMode.ToString() },
                { "Format", Format },
                { "RegistryMode", IsSLBService ? "Auto" : "Manual" },
                { "SubEnv", TestSubEnv }
            };

            if (!IsSLBService)
                clientInfo["ServiceUrl"] = BaseUri;

            return clientInfo;
        }

        protected internal Dictionary<string, string> GetClientInfo(ExecutionContext context)
        {
            Dictionary<string, string> clientInfo = GetClientInfo();
            clientInfo["Format"] = context.Format;
            clientInfo["Operation"] = context.Operation;
            clientInfo["InvocationMode"] = context.ExecutionMode;
            if (IsSLBService && context.ServiceUrl != null)
                clientInfo["ServiceUrl"] = context.ServiceUrl;
            return clientInfo;
        }

        protected internal string GetLogTitle(string title)
        {
            if (!string.IsNullOrWhiteSpace(ServiceContact))
            {
                if (!string.IsNullOrWhiteSpace(title))
                {
                    title = title.Trim();
                    if (!title.EndsWith("."))
                        title += ".";
                    title += " ";
                }

                title += "Service Owner: " + ServiceContact;
            }

            return title;
        }

       

        private void InitMetadata()
        {
            Type type = typeof(DerivedClient);
            if (!serviceMetadataCache.ContainsKey(type.FullName) || serviceMetadataCache[type.FullName] == null)
            {
                lock (serviceMetadataCache)
                {
                    if (!serviceMetadataCache.ContainsKey(type.FullName) || serviceMetadataCache[type.FullName] == null)
                    {
                        ConcurrentDictionary<string, string> metadata = new ConcurrentDictionary<string, string>();
                        List<string> constantFieldNames = new List<string>() 
                        {
                            OriginalServiceNameFieldName, OriginalServiceNamespaceFieldName, CCodeGeneratorVersionFieldName, OriginalServiceTypeFieldName
                        };
                        FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static);
                        foreach (FieldInfo fi in fieldInfos)
                        {
                            if (fi.IsLiteral && !fi.IsInitOnly && constantFieldNames.Contains(fi.Name))
                            {
                                string fieldValue = (string)fi.GetRawConstantValue();
                                metadata[fi.Name] = fieldValue;
                            }
                        }

                        if (!metadata.ContainsKey(CCodeGeneratorVersionFieldName))
                            metadata[CCodeGeneratorVersionFieldName] = "1.0.0.0";

                        if (!metadata.ContainsKey(OriginalServiceTypeFieldName))
                            metadata[OriginalServiceTypeFieldName] = NonSLBServiceType;

                        if (metadata.Count != constantFieldNames.Count)
                            throw new Exception(
                                string.Format(
                                    "Service name and namespace constants are not in the generated service client code: {0}, {1}",
                                    OriginalServiceNameFieldName,
                                    OriginalServiceNamespaceFieldName));

                        serviceMetadataCache[type.FullName] = metadata;
                    }
                }
            }
            ServiceName = serviceMetadataCache[type.FullName][OriginalServiceNameFieldName];
            ServiceNamespace = serviceMetadataCache[type.FullName][OriginalServiceNamespaceFieldName];
            CCodeGeneratorVersion = serviceMetadataCache[type.FullName][CCodeGeneratorVersionFieldName];
            ServiceType = serviceMetadataCache[type.FullName][OriginalServiceTypeFieldName];
            IsSLBService = string.Equals(ServiceType, SLBServiceType, StringComparison.OrdinalIgnoreCase);

            ServiceFullName = ServiceUtils.RefineServiceName(ServiceNamespace, ServiceName);
        }

      

     

        private void InitThreadLocalBaseUri(ClientExecutionContext context)
        {
            _threadLocalBaseUri.Value = context.ServiceUrl;
        }

        private ClientExecutionContext CreateExecutionContext(string operation, object requestObject, string executionMode)
        {
            string serviceUrl = GetServiceUrl(requestContext);//从consul获取到服务列表
            _threadLocalBaseUri.Value = serviceUrl;
            var format = Format;
            return new ClientExecutionContext()
            {
                ServiceName = ServiceName,
                ServiceNamespace = ServiceNamespace,
                Format = format,
                ServiceContact = ServiceContact,
                Operation = operation,
                OperationKey = OperationKeys.GetOrAdd(operation, GetOperationKey),
                ExecutionMode = executionMode,
                Request = requestObject,
                ServiceKey = ServiceFullName,
                CallFormat = CallFormats[format],
                ServiceUrl = serviceUrl,
                Host = GetCHystrixUrlKey(serviceUrl),
            };
        }

        private void ApplyWebResponseFilters(WebResponse webResponse)
        {
            if (!(webResponse is HttpWebResponse))
                return;

            var localFilter = LocalHttpWebResponseFilter;
            if (localFilter != null)
                localFilter((HttpWebResponse)webResponse);

            var filter = HttpWebResponseFilter;
            if (filter != null)
                filter((HttpWebResponse)webResponse);
        }

        private void ApplyWebRequestFilters(HttpWebRequest client)
        {
            var localFilter = LocalHttpWebRequestFilter;
            if (localFilter != null)
                localFilter(client);

            var filter = HttpWebRequestFilter;
            if (filter != null)
                filter(client);
        }

        private void ApplyRequestFilters(string serviceName, string serviceNamespace, string operationName, object requestObject)
        {
            var localFilter = LocalRequestFilter;
            if (localFilter != null)
                localFilter(serviceName, serviceNamespace, operationName, requestObject);

            var filter = RequestFilter;
            if (filter != null)
                filter(serviceName, serviceNamespace, operationName, requestObject);
        }

        private void ApplyResponseFilters(string serviceName, string serviceNamespace, string operationName, object responseObject)
        {
            var localFilter = LocalResponseFilter;
            if (localFilter != null)
                localFilter(serviceName, serviceNamespace, operationName, responseObject);

            var filter = ResponseFilter;
            if (filter != null)
                filter(serviceName, serviceNamespace, operationName, responseObject);
        }

        private void ApplyRequestEndFilter(ExecutionContext context)
        {
            Action<ExecutionContext> localFilter = LocalRequestEndFilter;
            if (localFilter != null)
                localFilter(context);

            Action<ExecutionContext> filter = RequestEndFilter;
            if (filter != null)
                filter(context);
        }

        private void ApplyRequestEndFilterSafe(ExecutionContext context)
        {
            try
            {
                ApplyRequestEndFilter(context);
            }
            catch (Exception ex)
            {
                Dictionary<string, string> addtionalInfo = GetClientInfo(context);
                addtionalInfo["ErrorCode"] = "FXD301041";
                log.Error("Client Request End Filter execute failed.", ex, addtionalInfo);
            }
        }

        private string GetOperationKey(string operation)
        {
            return string.Format("{0}.{1}", ServiceFullName, operation.ToLower());
        }

        private string GetCHystrixUrlKey(String url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            return CHystrixUrlInstanceKeyCache.GetOrAdd(url, key =>
                {
                    try
                    {
                        Uri uri = new Uri(key.Trim());
                        return uri.Host;
                    }
                    catch (Exception ex)
                    {
                        log.Warn("url " + key + " is invalid. use null url key.", ex);
                        return null;
                    }
                });
        }

        internal string InitializeCallFormat(IConfiguration configuration)
        {
            if (IsSLBService)
            {
                return Format = InitializeCallFormat(configuration, JavaCallFormatSettingKey, BaijiJsonClientCallFormat.ContentFormat, IsValidJavaCallFormat);
            }
            else
            {
                return Format = InitializeCallFormat(configuration, CallFormatSettingKey, XmlClientCallFormat.ContentFormat, IsValidCallFormat);
            }
        }

        private string InitializeCallFormat(IConfiguration configuration, string settingKey, string defaultFormat, Func<string, bool> predicate)
        {
            string callFormatSettingKey = GetServiceSettingKey(settingKey, ServiceName, ServiceNamespace);
            string specificCallFormat = configuration[callFormatSettingKey];
            if (!string.IsNullOrWhiteSpace(specificCallFormat))
            {
                var message0 = string.Format("{0} has been set to {1}", callFormatSettingKey, specificCallFormat);
                log.Info(message0, GetClientInfo().AddErrorCode("FXD301044"));
                specificCallFormat = specificCallFormat.Trim().ToLower();

                if (predicate(specificCallFormat))
                    return specificCallFormat;
            }

            string globalCallFormat = configuration[settingKey];
            if (!string.IsNullOrWhiteSpace(globalCallFormat))
            {
                var message1 = string.Format("{0} has been set to {1}", settingKey, globalCallFormat);
                log.Info(message1, GetClientInfo().AddErrorCode("FXD301044"));
                globalCallFormat = globalCallFormat.Trim().ToLower();

                if (predicate(globalCallFormat))
                    return globalCallFormat;
            }

            if (specificCallFormat != null || globalCallFormat != null)
            {
                var messageFormat = "Invalid format value. Specific: {0}, Global: {1}. Fall back to default format: {2}.";
                var message2 = string.Format(messageFormat, specificCallFormat, globalCallFormat, defaultFormat);
                log.Warn("Invalid Format", message2, GetClientInfo().AddErrorCode("FXD301044"));
            }
            return defaultFormat;
        }
    }
}
