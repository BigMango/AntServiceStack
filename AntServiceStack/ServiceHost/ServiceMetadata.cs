using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using AntServiceStack.Common;
using AntServiceStack.Common.Extensions;
using AntServiceStack.Common.Configuration;
using AntServiceStack.Common.Utils;
using AntServiceStack.Text;
using Freeway.Logging;
using AntServiceStack.WebHost.Endpoints;
using AntServiceStack.WebHost.Endpoints.Extensions;
using AntServiceStack.Common.Hystrix;
using AntServiceStack.Common.Message;
using AntServiceStack.Common.Hystrix.Atomic;
using AntServiceStack.WebHost.Endpoints.Config;

namespace AntServiceStack.ServiceHost
{
    /// <summary>
    /// 包含了 请求的类型 路由 请求的返回类型 请求的method class等所有信息
    /// </summary>
    public class ServiceMetadata
    {
        internal static readonly ILog Log = LogManager.GetLogger(typeof(ServiceMetadata));

        public const string SERVICE_REGISTRY_ENV_KEY = "SOA.ServiceRegistry.Service.TestSubEnv";
        public const string DefaultOperationTimeoutMapSettingKey = "SOA.OperationTimeoutMap";
        public const string DefaultCircuitBreakerForceClosedSettingKey = "SOA.CircuitBreakerForceClosed";
        public const string DefaultLogErrorWithRequestInfoSettingKey = "SOA.LogErrorWithRequestInfo";
        public const string DefaultServicePathMapSettingKey = "SOA.ServicePathMap";
        public const string DefaultLogCommonRequestInfoSettingKey = "SOA.LogCommonRequestInfo";
        public const string DefaultLogH5HeadExtensionDataSettingKey = "SOA.LogH5HeadExtensionData";
        public const string DefaultUseChunkedTransferEncodingSettingKey = "SOA.UseChunkedTransferEncoding";
        public const string DefaultCheckConnectionMaxRequestCountSettingKey = "SOA.CheckConnectionMaxRequestCount";
        public const string DefaultConnectionMaxRequestCountSettingKey = "SOA.ConnectionMaxRequestCount";
        internal const string DefaultDeserializeRequestUseMemoryStreamSettingKey = "SOA.DeserializeRequestUseMemoryStream";
        internal const string DefaultEnableMetadataFeatureConfigKey = "SOA.EnableMetadataFeature";

        public const string ServicePathPattern = @"^[a-zA-Z0-9\-_]+$";

        public static readonly string EmptyServicePath = string.Empty;

        public const string AnonymousServiceName = "AnonymousService";

        public const int MinConnectionMaxRequestCount = 1 * 500;

        public const int FrameworkDefaultConnectionMaxRequestCount = 2 * 1000;

        public static readonly string DefaultServicePath = EmptyServicePath;

        public static readonly Dictionary<string, string> DefaultOperationTimeoutMap;

        public static readonly bool DefaultCircuitBreakerForceClosed;

        public static readonly bool DefaultLogErrorWithRequestInfo;

        public static readonly bool DefaultLogCommonRequestInfo;
        public static readonly bool DefaultLogH5HeadExtensionData;

        public static readonly bool DefaultUseChunkedTransferEncoding;

        public static readonly bool DefaultCheckConnectionMaxRequestCount;
        public static readonly int DefaultConnectionMaxRequestCount;
        internal static readonly bool DefaultDeserializeRequestUseMemoryStream;
        internal static readonly bool DefaultMetadataFeatureEnabled;
        internal static readonly string DefaultServiceTestSubEnv;

        static ServiceMetadata()
        {
            string servicePathSettingValue = ConfigUtils.GetNullableAppSetting(DefaultServicePathMapSettingKey);
            if (!string.IsNullOrWhiteSpace(servicePathSettingValue))
            {
                string servicePath = servicePathSettingValue.Trim().ToLower();
                if (!Regex.IsMatch(servicePath, ServicePathPattern))
                    throw new Exception("ServicePathMap setting is invalid: " + servicePathSettingValue);

                DefaultServicePath = servicePath;
            }

            var opTimeoutMapSetting = ConfigUtils.GetNullableAppSetting(DefaultOperationTimeoutMapSettingKey);
            DefaultOperationTimeoutMap = ConfigUtils.GetDictionaryFromAppSettingValue(opTimeoutMapSetting);

            bool.TryParse(ConfigUtils.GetNullableAppSetting(DefaultCircuitBreakerForceClosedSettingKey), out DefaultCircuitBreakerForceClosed);

            bool.TryParse(ConfigUtils.GetNullableAppSetting(DefaultLogErrorWithRequestInfoSettingKey), out DefaultLogErrorWithRequestInfo);

            bool.TryParse(ConfigUtils.GetNullableAppSetting(DefaultCheckConnectionMaxRequestCountSettingKey), out DefaultCheckConnectionMaxRequestCount);

            if (!int.TryParse(ConfigUtils.GetNullableAppSetting(DefaultConnectionMaxRequestCountSettingKey), out DefaultConnectionMaxRequestCount))
                DefaultConnectionMaxRequestCount = FrameworkDefaultConnectionMaxRequestCount;//最大2000
            else if (DefaultConnectionMaxRequestCount < MinConnectionMaxRequestCount)
                DefaultConnectionMaxRequestCount = MinConnectionMaxRequestCount;

            if (!bool.TryParse(ConfigUtils.GetNullableAppSetting(DefaultLogCommonRequestInfoSettingKey), out DefaultLogCommonRequestInfo))
                DefaultLogCommonRequestInfo = true;
            bool.TryParse(ConfigUtils.GetNullableAppSetting(DefaultLogH5HeadExtensionDataSettingKey), out DefaultLogH5HeadExtensionData);

            bool.TryParse(ConfigUtils.GetNullableAppSetting(DefaultUseChunkedTransferEncodingSettingKey), out DefaultUseChunkedTransferEncoding);

            bool.TryParse(ConfigUtils.GetNullableAppSetting(DefaultDeserializeRequestUseMemoryStreamSettingKey), out DefaultDeserializeRequestUseMemoryStream);

            if (!bool.TryParse(ConfigUtils.GetNullableAppSetting(DefaultEnableMetadataFeatureConfigKey), out DefaultMetadataFeatureEnabled))
                DefaultMetadataFeatureEnabled = true;

            DefaultServiceTestSubEnv = ServiceConfig.Instance.ConfigurationManager.GetPropertyValue(SERVICE_REGISTRY_ENV_KEY, "");
            DefaultServiceTestSubEnv = string.IsNullOrWhiteSpace(DefaultServiceTestSubEnv) ? null : DefaultServiceTestSubEnv.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(DefaultServiceTestSubEnv))
                DefaultServiceTestSubEnv = "dev";
        }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; private set; }//服务名称
        /// <summary>
        /// 服务空间名称
        /// </summary>
        public string ServiceNamespace { get; private set; }//服务空间名称
        /// <summary>
        /// 服务的子环境
        /// </summary>
        internal string ServiceTestSubEnv { get; private set; }//服务的子环境
        /// <summary>
        /// soa框架的版本号
        /// </summary>
        public string AntServiceStackVersion { get; private set; }//soa框架的版本号
        /// <summary>
        /// Codegen工具的版本号
        /// </summary>
        public string AntCodeGenVersion { get; private set; }//Codegen工具的版本号

        /// <summary>
        /// metric前缀 soa.service
        /// </summary>
        public string ServiceMetricPrefix { get; private set; }
        /// <summary>
        /// 服务二级地址
        /// </summary>
        public string ServicePath { get; private set; }//服务二级地址
        /// <summary>
        /// 服务全名称
        /// </summary>
        public string FullServiceName { get; private set; }//服务全名称

        /// <summary>
        /// 服务名称 格式为：servicename + "." + namespace经过改造
        /// 例如 namespace为 http://soa.ant.com/innovationwork/CloudBag/v1  servicename为 CloudBagRestFulApi 
        /// 变成  cloudbagrestfulapi.soa.ant.com.innovationwork.CloudBag.v1
        /// </summary>
        public string RefinedFullServiceName { get; private set; }
        /// <summary>
        /// 是否是默认服务 也就是 ServicePath 为空
        /// </summary>
        public bool IsDefaultService { get; private set; }//是否是默认服务 也就是 ServicePath 为空
        /// <summary>
        /// 方法集合
        /// </summary>
        public Dictionary<string, Operation> OperationNameMap { get; protected set; }//方法集合
        public bool LogErrorWithRequestInfo { get; set; }
        public bool LogCommonRequestInfo { get; set; }
        public bool LogH5HeadExtensionData { get; set; }

        /// <summary>
        /// 电容器开关 已经默认开启了Circuit Breaker 启用电路保护功能 默认值为false
        /// 当连续10s内服务执行错误比率达到50%时，自动打开电路开关（熔断），阻止请求执行（直接返回SLA错误给客户端），直到服务恢复处理能力
        /// 1.自动熔断后，5s后放一个请求进入服务实现执行
        ///     1.1执行成功，闭合开关，恢复处理能力
        ///     1.2执行失败，5s后继续重试
        /// 5.超过Timeout时间限制的请求执行会被标记为Timeout（一种错误），但不会中止请求
        /// </summary>
        public bool CircuitBreakerForceClosed { get; set; }
        public bool UseChunkedTransferEncoding { get; protected set; }
        /// <summary> 
        /// 是否开启对IP的最大连接数的限制 默认关闭
        /// </summary>
        public bool CheckConnectionMaxRequestCount { get; protected set; }
        /// <summary>
        /// 最大连接数 如果没有配置就默认 2000
        /// </summary>
        public int ConnectionMaxRequestCount { get; protected set; }

        /// <summary>
        /// 序列化requestStream 是否用memorystream的方式
        /// </summary>
        internal bool DeserializeRequestUseMemoryStream { get; set; }
        /// <summary>
        /// Request类型的集合
        /// </summary>
        public HashSet<Type> RequestTypes { get; protected set; }
        /// <summary>
        /// Controller Type集合
        /// </summary>
        public List<Type> ServiceTypes { get; private set; }
        /// <summary>
        /// Response类型的集合
        /// </summary>
        public HashSet<Type> ResponseTypes { get; protected set; }
        public ServiceRoutes Routes { get; set; }
        /// <summary>
        ///  <add key="SOA.OperationTimeoutMap" value="CheckHealth:1000,GetItems:2000" />
        /// 配置操作执行超时时间（默认20000ms，单位ms，注意这个超时不是整正意义上的超时，只是在统计时把超过时间的请求处理结果当成超时统计）
        /// </summary>
        public Dictionary<string, string> OperationTimeoutMap { get; protected set; }
        internal MessageLogConfig ServiceMessageLogConfig { get; private set; }
        /// <summary>
        /// Meta页面是否启用 默认开启
        /// </summary>
        public bool MetadataFeatureEnabled { get; private set; }

        private AtomicInteger serviceConcurrentExecutionCount = new AtomicInteger(0);
        private AtomicInteger serviceMaxConcurrentExecutionCount = new AtomicInteger(0);
        /// <summary>
        /// 服务当前请求的并发量
        /// </summary>
        internal int ServiceCurrentConcurrentExecutionCount { get { return serviceConcurrentExecutionCount.Value; } }
        /// <summary>
        /// 服务请求的最大并发量
        /// </summary>
        internal int ServiceMaxConcurrentExecutionCount { get { return serviceMaxConcurrentExecutionCount.Value; } }

        public ServiceMetadata(Type serviceType, string serviceName, string serviceNamespace, string codeGeneratorVersion)
        {
            this.RequestTypes = new HashSet<Type>();
            this.ResponseTypes = new HashSet<Type>();
            this.OperationNameMap = new Dictionary<string, Operation>();
            this.Routes = new ServiceRoutes();
            this.ServiceTypes = new List<Type>() { serviceType };
            this.ServiceName = string.IsNullOrWhiteSpace(serviceName) ? AnonymousServiceName : serviceName;
            this.ServiceNamespace = string.IsNullOrWhiteSpace(serviceNamespace) ? "http://soa.ant.com/anonymous" : serviceNamespace;
            this.FullServiceName = ServiceName + "{" + ServiceNamespace + "}";
            this.RefinedFullServiceName = ServiceUtils.RefineServiceName(ServiceNamespace, ServiceName);
            this.ServiceMetricPrefix = "soa.service";
            this.AntServiceStackVersion = typeof(ServiceMetadata).Assembly.GetName().Version.ToString();
            this.AntCodeGenVersion = codeGeneratorVersion;

            //判断appseting里面是否设置了特殊的访问前缀的 key
            //如果没有就默认为空的
            string servicePathSettingValue = ConfigUtils.GetNullableAppSetting(GetServiceSpecificSettingKey(DefaultServicePathMapSettingKey));
            this.IsDefaultService = string.IsNullOrWhiteSpace(servicePathSettingValue);
            if (this.IsDefaultService)
                this.ServicePath = DefaultServicePath;
            else
            {
                string servicePath = servicePathSettingValue.Trim().ToLower();
                if (!Regex.IsMatch(servicePath, ServicePathPattern))
                    throw new Exception("ServicePathMap setting is invalid: " + servicePathSettingValue);

                this.ServicePath = servicePath;
            }

            //判断appseting里面是否设置了方法执行的timeout
            string operationTimeoutMapSettingKey = GetServiceSpecificSettingKey(DefaultOperationTimeoutMapSettingKey);
            var opTimeoutMapSetting = ConfigUtils.GetNullableAppSetting(operationTimeoutMapSettingKey);
            this.OperationTimeoutMap = ConfigUtils.GetDictionaryFromAppSettingValue(opTimeoutMapSetting);
            foreach (var item in DefaultOperationTimeoutMap)
            {
                if (!this.OperationTimeoutMap.ContainsKey(item.Key))
                    this.OperationTimeoutMap[item.Key] = item.Value;
            }

            #region 电容器开关

            string circuitBreakerForceClosedSettingKey = GetServiceSpecificSettingKey(DefaultCircuitBreakerForceClosedSettingKey);
            bool circuitBreakerForceClosed;
            if (!bool.TryParse(ConfigUtils.GetNullableAppSetting(circuitBreakerForceClosedSettingKey), out circuitBreakerForceClosed))
                circuitBreakerForceClosed = DefaultCircuitBreakerForceClosed;
            this.CircuitBreakerForceClosed = circuitBreakerForceClosed;

            #endregion

            string logErrorWithRequestInfoSettingKey = GetServiceSpecificSettingKey(DefaultLogErrorWithRequestInfoSettingKey);
            bool logErrorWithRequestInfo;
            if (!bool.TryParse(ConfigUtils.GetNullableAppSetting(logErrorWithRequestInfoSettingKey), out logErrorWithRequestInfo))
                logErrorWithRequestInfo = DefaultLogErrorWithRequestInfo;
            this.LogErrorWithRequestInfo = logErrorWithRequestInfo;

            string logH5HeadExtensionDataSettingKey = GetServiceSpecificSettingKey(DefaultLogH5HeadExtensionDataSettingKey);
            bool logH5HeadExtensionData;
            if (!bool.TryParse(ConfigUtils.GetNullableAppSetting(logH5HeadExtensionDataSettingKey), out logH5HeadExtensionData))
                logH5HeadExtensionData = DefaultLogH5HeadExtensionData;
            this.LogH5HeadExtensionData = logH5HeadExtensionData;

            string logCommonRequestInfoSettingKey = GetServiceSpecificSettingKey(DefaultLogCommonRequestInfoSettingKey);
            bool logCommonRequestInfo;
            if (!bool.TryParse(ConfigUtils.GetNullableAppSetting(logCommonRequestInfoSettingKey), out logCommonRequestInfo))
                logCommonRequestInfo = DefaultLogCommonRequestInfo;
            this.LogCommonRequestInfo = logCommonRequestInfo;

            string useChunkedTransferEncodingSettingKey = GetServiceSpecificSettingKey(DefaultUseChunkedTransferEncodingSettingKey);
            bool useChunkedTransferEncoding;
            if (!bool.TryParse(ConfigUtils.GetNullableAppSetting(useChunkedTransferEncodingSettingKey), out useChunkedTransferEncoding))
                useChunkedTransferEncoding = DefaultUseChunkedTransferEncoding;
            this.UseChunkedTransferEncoding = useChunkedTransferEncoding;

            string serviceTestSubEnvSettingKey = GetServiceSpecificSettingKey(SERVICE_REGISTRY_ENV_KEY);
            ServiceTestSubEnv = ConfigUtils.GetNullableAppSetting(serviceTestSubEnvSettingKey);
            if (string.IsNullOrWhiteSpace(ServiceTestSubEnv))
                ServiceTestSubEnv = DefaultServiceTestSubEnv;
            else
                ServiceTestSubEnv = ServiceTestSubEnv.Trim().ToLower();



            #region 对于每个单个IP的连接数限制
            //是否打开开关的配置
            string checkConnectionMaxRequestCountSettingKey = GetServiceSpecificSettingKey(DefaultCheckConnectionMaxRequestCountSettingKey);
            bool checkConnectionMaxRequestCount;
            if (!bool.TryParse(ConfigUtils.GetNullableAppSetting(checkConnectionMaxRequestCountSettingKey), out checkConnectionMaxRequestCount))
                checkConnectionMaxRequestCount = DefaultCheckConnectionMaxRequestCount;//默认false
            this.CheckConnectionMaxRequestCount = checkConnectionMaxRequestCount;


            string connectionMaxRequestCountSettingKey = GetServiceSpecificSettingKey(DefaultConnectionMaxRequestCountSettingKey);
            int connectionMaxRequestCount;
            if (!int.TryParse(ConfigUtils.GetNullableAppSetting(connectionMaxRequestCountSettingKey), out connectionMaxRequestCount))
                connectionMaxRequestCount = DefaultConnectionMaxRequestCount; // 如果没有配置就默认 2000
            else if (connectionMaxRequestCount < MinConnectionMaxRequestCount)
                connectionMaxRequestCount = MinConnectionMaxRequestCount;//如果配置了低于500 就取500 (最低不能低于500)
            this.ConnectionMaxRequestCount = connectionMaxRequestCount;

            #endregion

            string deserializeRequestUseMemoryStreamSettingKey = GetServiceSpecificSettingKey(DefaultDeserializeRequestUseMemoryStreamSettingKey);
            bool deserializeRequestUseMemoryStream;
            if (!bool.TryParse(ConfigUtils.GetNullableAppSetting(deserializeRequestUseMemoryStreamSettingKey), out deserializeRequestUseMemoryStream))
                deserializeRequestUseMemoryStream = DefaultDeserializeRequestUseMemoryStream;
            this.DeserializeRequestUseMemoryStream = deserializeRequestUseMemoryStream;

            string metadataFeatureEnabledSettingKey = GetServiceSpecificSettingKey(DefaultEnableMetadataFeatureConfigKey);
            bool metadataFeatureEnabled;
            if (!bool.TryParse(ConfigUtils.GetNullableAppSetting(metadataFeatureEnabledSettingKey), out metadataFeatureEnabled))
                metadataFeatureEnabled = DefaultMetadataFeatureEnabled;
            this.MetadataFeatureEnabled = metadataFeatureEnabled;

            MessageSensitivityAttribute sensitivityAttribute = null;
            if (serviceType != null)
                sensitivityAttribute = serviceType.GetCustomAttributes(true).OfType<MessageSensitivityAttribute>().FirstOrDefault();
            this.ServiceMessageLogConfig =  AttributeToConfig(sensitivityAttribute);
        }

        public void MergeData(ServiceMetadata other)
        {
            this.ServiceTypes.AddRange(other.ServiceTypes);

            foreach (var item in other.RequestTypes)
                this.RequestTypes.Add(item);

            foreach (var item in other.ResponseTypes)
                this.ResponseTypes.Add(item);

            foreach (var item in other.OperationTimeoutMap)
                this.OperationTimeoutMap[item.Key] = item.Value;

            foreach (var item in other.OperationNameMap)
                this.OperationNameMap.Add(item.Key, item.Value);

            this.Routes.RestPaths.AddRange(other.Routes.RestPaths);
        }

        public string GetServiceSpecificSettingKey(string settingKey)
        {
            return settingKey + ":" + FullServiceName;
        }

        public IEnumerable<Operation> Operations
        {
            get { return OperationNameMap.Values; }
        }

        public void Add(Type serviceType, MethodInfo mi, Type requestType, Type responseType, bool isAsync)
        {
            this.RequestTypes.Add(requestType);

            object[] methodCustomAttributes = mi.GetCustomAttributes(true);
            object[] classCustomAttributes = serviceType.GetCustomAttributes(true);

            var restrictTo = methodCustomAttributes.OfType<RestrictAttribute>().FirstOrDefault() 
                ?? classCustomAttributes.OfType<RestrictAttribute>().FirstOrDefault();

            var operation = new Operation
            {
                Name = mi.Name,
                Key = string.Format("{0}.{1}", this.RefinedFullServiceName, mi.Name.ToLower()),
                ServiceType = serviceType,
                Method = mi,
                RequestType = requestType,
                ResponseType = responseType,
                RestrictTo = restrictTo,
                Routes = new List<RestPath>(),
                Descritpion = mi.GetDescription(),
                IsAsync = isAsync,
            };

            var hystrixCommandPropertiesSetter = new HystrixCommandPropertiesSetter();

            hystrixCommandPropertiesSetter.WithCircuitBreakerForceClosed(CircuitBreakerForceClosed);

            // operation timeout attribute override
            var hystrixAttribute = methodCustomAttributes.OfType<HystrixAttribute>().FirstOrDefault();
            if (hystrixAttribute != null && hystrixAttribute.Timeout > 0)
            {
                hystrixCommandPropertiesSetter.WithExecutionIsolationThreadTimeoutInMilliseconds(hystrixAttribute.Timeout);
                Log.Info(string.Format("Timeout for operation {0} is overridden by attribute to value {1}", mi.Name, hystrixAttribute.Timeout),
                    new Dictionary<string, string>() 
                    {
                        {"ErrorCode", "FXD300046"}
                    });
            }

            // operation timeout setting override in appSettings
            if (OperationTimeoutMap != null && OperationTimeoutMap.ContainsKey(mi.Name))
            {
                var timeoutSetting = OperationTimeoutMap[mi.Name];
                try
                {
                    var timeout = int.Parse(timeoutSetting);
                    if (timeout > 0)
                    {
                        hystrixCommandPropertiesSetter.WithExecutionIsolationThreadTimeoutInMilliseconds(timeout);
                        Log.Info(string.Format("Timeout for operation {0} is overridden in appSettings to value {1}", mi.Name, timeout),
                            new Dictionary<string, string>() 
                            {
                                {"ErrorCode", "FXD300047"}
                            });
                    }
                    else
                    {
                        Log.Error(
                            string.Format("Invalid operation timeout setting {0}:{1} in appSettings", mi.Name, timeoutSetting),
                            new Dictionary<string, string>()
                            {
                                { "ErrorCode", "FXD300006" }
                            });
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(
                        string.Format("Invalid operation timeout setting {0}:{1} in appSettings", mi.Name, timeoutSetting),
                        ex,
                        new Dictionary<string, string>()
                        {
                            { "ErrorCode", "FXD300006" }
                        });
                }
            }

            var hystrixCommnad = new HystrixCommand(ServicePath, mi.Name, ServiceName, FullServiceName, ServiceMetricPrefix, hystrixCommandPropertiesSetter);
            operation.HystrixCommand = hystrixCommnad;

            operation.RequestFilters = methodCustomAttributes.OfType<IHasRequestFilter>().ToList();
            operation.RequestFilters.AddRange(classCustomAttributes.OfType<IHasRequestFilter>().ToList());
            operation.ResponseFilters = methodCustomAttributes.OfType<IHasResponseFilter>().ToList();
            operation.ResponseFilters.AddRange(classCustomAttributes.OfType<IHasResponseFilter>().ToList());

            var sensitivityAttribute = methodCustomAttributes.OfType<MessageSensitivityAttribute>().FirstOrDefault();
            operation.OperationMessageLogConfig = AttributeToConfig(sensitivityAttribute);

            this.OperationNameMap[mi.Name.ToLower()] = operation;

            if (responseType != null)
            {
                this.ResponseTypes.Add(responseType);
            }
        }

        public void AfterInit()
        {
            foreach (var restPath in Routes.RestPaths)
            {
                Operation operation;
                if (!OperationNameMap.TryGetValue(restPath.OperationName.ToLower(), out operation))
                    continue;

                operation.Routes.Add(restPath);
            }
        }

        public List<OperationDto> GetOperationDtos()
        {
            return OperationNameMap.Values
                .SafeConvertAll(x => x.ToOperationDto())
                .OrderBy(x => x.Name)
                .ToList();
        }

        public Operation GetOperationByOpName(string operationName)
        {
            Operation op;
            OperationNameMap.TryGetValue(operationName.ToLower(), out op);
            return op;
        }

        public Type GetRequestTypeByOpName(string operationName)
        {
            Operation operation;
            OperationNameMap.TryGetValue(operationName.ToLower(), out operation);
            return operation != null ? operation.RequestType : null;
        }

        public Type GetServiceTypeByOpName(string operationName)
        {
            Operation operation;
            OperationNameMap.TryGetValue(operationName.ToLower(), out operation);
            return operation != null ? operation.ServiceType: null;
        }

        public Type GetResponseTypeByOpName(string operationName)
        {
            Operation operation;
            OperationNameMap.TryGetValue(operationName.ToLower(), out operation);
            return operation != null ? operation.ResponseType : null;
        }

        public HystrixCommand GetHystrixCommandByOpName(string operationName)
        {
            Operation operation;
            OperationNameMap.TryGetValue(operationName.ToLower(), out operation);
            return operation != null ? operation.HystrixCommand : null;
        }

        public bool HasOperation(string operationName)
        {
            if (operationName == null)
                return false;
            return OperationNameMap.ContainsKey(operationName.ToLower());
        }

        public List<Type> GetAllTypes()
        {
            var allTypes = new List<Type>(RequestTypes);
            foreach (var responseType in ResponseTypes)
            {
                allTypes.AddIfNotExists(responseType);
            }
            return allTypes;
        }

        public List<string> GetAllOperationNames()
        {
            return Operations.Select(x => x.Name).OrderBy(operation => operation).ToList();
        }

        public bool IsVisible(IHttpRequest httpReq, Operation operation)
        {
            if (EndpointHost.Config != null && !EndpointHost.Config.EnableAccessRestrictions)
                return true;

            if (operation.RestrictTo == null) return true;

            //Less fine-grained on /metadata pages. Only check Network and Format
            var reqAttrs = httpReq.GetAttributes();
            var showToNetwork = CanShowToNetwork(operation, reqAttrs);
            return showToNetwork;
        }

        public bool IsVisible(IHttpRequest httpReq, Format format, string operationName)
        {
            if (EndpointHost.Config != null && !EndpointHost.Config.EnableAccessRestrictions)
                return true;

            Operation operation;
            OperationNameMap.TryGetValue(operationName.ToLower(), out operation);
            if (operation == null) return false;

            var isVisible = IsVisible(httpReq, operation);
            if (!isVisible) return false;

            if (operation.RestrictTo == null) return true;
            var allowsFormat = operation.RestrictTo.CanShowTo((EndpointAttributes)(long)format);
            return allowsFormat;
        }

        public bool CanAccess(IHttpRequest httpReq, Format format, string operationName)
        {
            var reqAttrs = httpReq.GetAttributes();
            return CanAccess(reqAttrs, format, operationName);
        }

        public bool CanAccess(EndpointAttributes reqAttrs, Format format, string operationName)
        {
            if (EndpointHost.Config != null && !EndpointHost.Config.EnableAccessRestrictions)
                return true;

            Operation operation;
            OperationNameMap.TryGetValue(operationName.ToLower(), out operation);
            if (operation == null) return false;

            if (operation.RestrictTo == null) return true;

            var allow = operation.RestrictTo.HasAccessTo(reqAttrs);
            if (!allow) return false;

            var allowsFormat = operation.RestrictTo.HasAccessTo((EndpointAttributes)(long)format);
            return allowsFormat;
        }

        public bool CanAccess(Format format, string operationName)
        {
            if (EndpointHost.Config != null && !EndpointHost.Config.EnableAccessRestrictions)
                return true;

            Operation operation;
            OperationNameMap.TryGetValue(operationName.ToLower(), out operation);
            if (operation == null) return false;

            if (operation.RestrictTo == null) return true;

            var allowsFormat = operation.RestrictTo.HasAccessTo((EndpointAttributes)(long)format);
            return allowsFormat;
        }

        private static bool CanShowToNetwork(Operation operation, EndpointAttributes reqAttrs)
        {
            if (reqAttrs.IsLocalhost())
                return operation.RestrictTo.CanShowTo(EndpointAttributes.Localhost)
                       || operation.RestrictTo.CanShowTo(EndpointAttributes.LocalSubnet);

            return operation.RestrictTo.CanShowTo(
                reqAttrs.IsLocalSubnet()
                    ? EndpointAttributes.LocalSubnet
                    : EndpointAttributes.External);
        }

        private static MessageLogConfig AttributeToConfig(MessageSensitivityAttribute attribute)
        {
            MessageLogConfig config = new MessageLogConfig();
            if (attribute == null)
                return config;

            switch (attribute.RequestSensitivity)
            {
                case SensitivityMode.Default:
                    config.IsRequestSensitive = null;
                    break;
                case SensitivityMode.Insensitive:
                    config.IsRequestSensitive = false;
                    break;
                case SensitivityMode.Sensitive:
                    config.IsRequestSensitive = true;
                    break;
            }

            switch (attribute.ResponseSensitivity)
            {
                case SensitivityMode.Default:
                    config.IsResponseSensitive = null;
                    break;
                case SensitivityMode.Insensitive:
                    config.IsResponseSensitive = false;
                    break;
                case SensitivityMode.Sensitive:
                    config.IsResponseSensitive = true;
                    break;
            }

            if (attribute.LogResponse)
                config.LogResponse = attribute.LogResponse;

            if (attribute.DisableLog)
                config.DisableLog = attribute.DisableLog;

            return config;
        }

        internal bool CanLogRequest(string operationName)
        {
            MessageLogConfig operationMessageLogConfig = this.OperationNameMap[operationName.ToLower()].OperationMessageLogConfig;
            
            bool disableLog = operationMessageLogConfig.DisableLog ?? ServiceMessageLogConfig.DisableLog ?? MessageLogConfig.CurrentFrameworkDefaultMessageLogConfig.DisableLog ?? false;
            if (disableLog)
                return false;

            bool logRequest = operationMessageLogConfig.LogRequest ?? ServiceMessageLogConfig.LogRequest ?? MessageLogConfig.CurrentFrameworkDefaultMessageLogConfig.LogRequest ?? true;
            if (!logRequest)
                return false;

            bool isRequestSensitive;
            switch (EnvironmentUtility.CurrentEnv)
            {
                case EnvironmentUtility.TestEnv:
                case EnvironmentUtility.UatEnv:
                    isRequestSensitive = MessageLogConfig.CurrentFrameworkDefaultMessageLogConfig.IsRequestSensitive ?? false;
                    break;

                case EnvironmentUtility.ProdEnv:
                default:
                    isRequestSensitive = operationMessageLogConfig.IsRequestSensitive ?? ServiceMessageLogConfig.IsRequestSensitive ?? MessageLogConfig.CurrentFrameworkDefaultMessageLogConfig.IsRequestSensitive ?? true;
                    break;
            }

            return !isRequestSensitive;
        }

        internal bool CanLogResponse(string operationName)
        {
            MessageLogConfig operationMessageLogConfig = this.OperationNameMap[operationName.ToLower()].OperationMessageLogConfig;
            
            bool disableLog = operationMessageLogConfig.DisableLog ?? ServiceMessageLogConfig.DisableLog ?? MessageLogConfig.CurrentFrameworkDefaultMessageLogConfig.DisableLog ?? false;
            if (disableLog)
                return false;

            bool logResponse = operationMessageLogConfig.LogResponse ?? ServiceMessageLogConfig.LogResponse ?? MessageLogConfig.CurrentFrameworkDefaultMessageLogConfig.LogResponse ?? false;
            if (!logResponse)
                return false;

            bool isResponseSensitive;
            switch (EnvironmentUtility.CurrentEnv)
            {
                case EnvironmentUtility.TestEnv:
                case EnvironmentUtility.UatEnv:
                    isResponseSensitive = MessageLogConfig.CurrentFrameworkDefaultMessageLogConfig.IsResponseSensitive ?? false;
                    break;

                case EnvironmentUtility.ProdEnv:
                default:
                    isResponseSensitive = operationMessageLogConfig.IsResponseSensitive ?? ServiceMessageLogConfig.IsResponseSensitive ?? MessageLogConfig.CurrentFrameworkDefaultMessageLogConfig.IsResponseSensitive ?? true;
                    break;
            }

            return !isResponseSensitive;
        }

        /// <summary>
        /// 服务并发量自增
        /// </summary>

        internal void IncrementConcurrentExecutionCount()
        {
            var serviceCount = serviceConcurrentExecutionCount.IncrementAndGet();
            if (serviceCount > serviceMaxConcurrentExecutionCount.Value)
                serviceMaxConcurrentExecutionCount.GetAndSet(serviceCount);
        }

        /// <summary>
        /// 服务并发量自减
        /// </summary>
        internal void DecrementConcurrentExecutionCount()
        {
            serviceConcurrentExecutionCount.DecrementAndGet();
        }
    }

    public class Operation
    {
        /// <summary>
        /// 方法名称
        /// </summary>
        public string Name { get; set; }
        public string Key { get; set; }
        public Type ServiceType { get; set; }
        public MethodInfo Method { get; set; }
        public Type RequestType { get; set; }
        public Type ResponseType { get; set; }
        public RestrictAttribute RestrictTo { get; set; }
        public List<RestPath> Routes { get; set; }
        public bool IsOneWay { get { return ResponseType == null; } }
        public string Descritpion { get; set; }
        public List<IHasRequestFilter> RequestFilters {get; set;}
        public List<IHasResponseFilter> ResponseFilters { get; set; }
        public HystrixCommand HystrixCommand { get; set; }
        internal MessageLogConfig OperationMessageLogConfig { get; set; }
        internal bool IsAsync { get; set; }
    }

    [XmlType]
    public class OperationDto
    {
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string RequestName { get; set; }
        [XmlElement]
        public string ResponseName { get; set; }
        [XmlElement]
        public string ServiceName { get; set; }
        [XmlArray]
        public List<string> RestrictTo { get; set; }
        [XmlArray]
        public List<string> VisibleTo { get; set; }
        [XmlArray]
        public List<RouteItem> Routes { get; set; }
    }

    [XmlType]
    public class RouteItem
    {
        [XmlElement]
        public string Path { get; set; }

        [XmlElement]
        public string AllowedVerbs { get; set; }
    }

    public class XsdMetadata
    {
        public ServiceMetadata Metadata { get; set; }
        public bool Flash { get; set; }

        public XsdMetadata(ServiceMetadata metadata, bool flash = false)
        {
            Metadata = metadata;
            Flash = flash;
        }

        public List<Type> GetAllTypes()
        {
            var allTypes = new List<Type>(Metadata.RequestTypes);
            allTypes.AddRange(Metadata.ResponseTypes);
            return allTypes;
        }

        public List<string> GetReplyOperationNames(Format format)
        {
            return Metadata.OperationNameMap.Values
                .Where(x => EndpointHost.Config != null
                    && EndpointHost.Config.CreateMetadataPagesConfig(Metadata).CanAccess(format, x.Name))
                .Where(x => !x.IsOneWay)
                .Select(x => x.Name)
                .ToList();
        }

        public List<string> GetOneWayOperationNames(Format format)
        {
            return Metadata.OperationNameMap.Values
                .Where(x => EndpointHost.Config != null
                    && EndpointHost.Config.CreateMetadataPagesConfig(Metadata).CanAccess(format, x.Name))
                .Where(x => x.IsOneWay)
                .Select(x => x.Name)
                .ToList();
        }

        /// <summary>
        /// Gets the name of the base most type in the hierachy tree with the same.
        /// 
        /// We get an exception when trying to create a schema with multiple types of the same name
        /// like when inheriting from a DataContract with the same name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Type GetBaseTypeWithTheSameName(Type type)
        {
            var typesWithSameName = new Stack<Type>();
            var baseType = type;
            do
            {
                if (baseType.Name == type.Name)
                    typesWithSameName.Push(baseType);
            }
            while ((baseType = baseType.BaseType) != null);

            return typesWithSameName.Pop();
        }
    }

    public static class ServiceMetadataExtensions
    {
        public static OperationDto ToOperationDto(this Operation operation)
        {
            var to = new OperationDto
            {
                Name = operation.Name,
                RequestName = operation.RequestType.Name,
                ResponseName = operation.IsOneWay ? null : operation.ResponseType.Name,
                ServiceName = operation.ServiceType.Name,
            };

            if (operation.Routes != null && operation.Routes.Count > 0)
            {
                to.Routes = new List<RouteItem>();
                foreach (RestPath restPath in operation.Routes)
                {
                    to.Routes.Add(new RouteItem { Path = restPath.Path, AllowedVerbs = restPath.AllowedVerbs});
                }
            }

            if (operation.RestrictTo != null)
            {
                to.RestrictTo = operation.RestrictTo.AccessibleToAny.ToList().ConvertAll(x => x.ToString());
                to.VisibleTo = operation.RestrictTo.VisibleToAny.ToList().ConvertAll(x => x.ToString());
            }

            return to;
        }

        public static string GetDescription(this MethodInfo serviceMethodInfo)
        {
            var apiAttr = serviceMethodInfo.GetCustomAttributes(typeof(ApiAttribute), true).OfType<ApiAttribute>().FirstOrDefault();
            return apiAttr != null ? apiAttr.Description : "";
        }

        public static List<ApiMemberAttribute> GetApiMembers(this Type requestType)
        {
            var members = requestType.GetMembers(BindingFlags.Instance | BindingFlags.Public);
            List<ApiMemberAttribute> attrs = new List<ApiMemberAttribute>();
            foreach (var member in members)
            {
                var memattr = member.GetCustomAttributes(typeof(ApiMemberAttribute), true)
                    .OfType<ApiMemberAttribute>()
                    .Select(x => { x.Name = x.Name ?? member.Name; return x; });

                attrs.AddRange(memattr);
            }

            return attrs;
        }
    }
}
