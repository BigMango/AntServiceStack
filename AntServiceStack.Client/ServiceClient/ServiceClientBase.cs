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
using AntServiceStack.Common.Types;
using AntServiceStack.Common.Configuration;
using ExecutionContext = AntServiceStack.Common.Execution.ExecutionContext;
using AntServiceStack.Client.CHystrix;
using AntServiceStack.Client.Config;
using AntServiceStack.Common.ServiceClient;

namespace AntServiceStack.ServiceClient
{
    public abstract class ServiceClientBase
    {
        protected const string ServiceUniqueFormat = ":{0}{{{1}}}";

        protected const string SERVICE_REGISTRY_KEY = "SOA.ServiceRegistry.Url";
        internal const string SERVICE_REGISTRY_SUBENV_KEY = "SOA.ServiceRegistry.TestSubEnv";

        protected const string DefaultConnectionLimitSettingKey = "SOA.DefaultConnectionLimit";
        protected const string DefaultConnectionLeaseTimeoutSettingKey = "SOA.DefaultConnectionLeaseTimeout";
        protected const string DisableConnectionLeaseTimeoutSettingKey = "SOA.DisableConnectionLeaseTimeout";

        protected const int MinConnectionLeaseTimeout = 1 * 60 * 1000;
        protected const int FrameworkDefaultConnectionLeaseTimeout = 5 * 60 * 1000;

        protected const string LogErrorWithRequestInfoSettingKey = "SOA.LogErrorWithRequestInfo";
        protected const string LogWebExceptionAsErrorSettingKey = "SOA.LogWebExceptionAsError";
        protected const string LogCServiceExceptionAsErrorSettingKey = "SOA.LogCServiceExceptionAsError";
        protected const string CatIgnoreWarningWebExceptionSettingKey = "SOA.CatIgnoreWarningWebException";

        protected const string RequestTimeoutSettingKey = "SOA.RequestTimeout";
        protected const string RequestReadWriteTimeoutSettingKey = "SOA.RequestReadWriteTimeout";

        protected const string HandleServiceErrorManuallySettingKey = "SOA.HandleServiceErrorManually";

        protected const string EnableCHystrixSupportSettingKey = "SOA.EnableCHystrixSupport";
        protected const string EnableCHystrixSupportForIOCPAsyncSettingKey = "SOA.EnableCHystrixSupportForIOCPAsync";
        protected const string EnableTimeoutForIOCPAsyncSettingKey = "SOA.EnableTimeoutForIOCPAsync";
        protected const string CHystrixCommandMaxConcurrentCountSettingKey = "SOA.CHystrixCommandMaxConcurrentCount";
        protected const string CHystrixCommandKeyPrefixForSync = "soa.";
        protected const string CHystrixCommandKeyPrefixForIOCPAsync = "soa.iocp.";

        protected const string DisableAutoCompressionSettingKey = "SOA.DisableAutoCompression";
        protected const DecompressionMethods DefaultCompressionModes = DecompressionMethods.GZip | DecompressionMethods.Deflate;

        protected const string DeserializeResponseUseMemoryStreamSettingKey = "SOA.DeserializeResponseUseMemoryStream";

        protected static readonly bool DefaultDeserializeResponseUseMemoryStream;
        
        protected internal const string CallFormatSettingKey = "SOA.CallFormat";
        protected internal const string JavaCallFormatSettingKey = "SOA.CallFormat.Java";

        protected const long DEFAULT_METRIC_SENDING_INTERVAL = 60 * 1000;

        protected const string OriginalServiceNameFieldName = "OriginalServiceName";
        protected const string OriginalServiceNamespaceFieldName = "OriginalServiceNamespace";
        protected const string CCodeGeneratorVersionFieldName = "CodeGeneratorVersion";
        protected const string OriginalServiceTypeFieldName = "OriginalServiceType";
        protected internal const string NonSLBServiceType = "NonSLB";
        protected internal const string SLBServiceType = "SLB";

        protected const string AppIdSettingKey = "AppId";

        public const string DefaultHttpMethod = "POST";

        protected const int RegistrySyncRetryTimeLimitAtStart = 3;

        private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceClientBase));

        protected static readonly ConcurrentDictionary<string, IClientCallFormat> CallFormats = new ConcurrentDictionary<string, IClientCallFormat>();

        protected static readonly ConcurrentDictionary<string, ServiceClientBase> clientCache = new ConcurrentDictionary<string, ServiceClientBase>();
        protected static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> serviceMetadataCache = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();

        protected static readonly bool DefaultLogErrorWithRequestInfo;
        protected static readonly bool DefaultLogWebExceptionAsError = true;
        protected static readonly bool DefaultLogCServiceExceptionAsError = true;
        protected static readonly bool DefaultCatIgnoreWarningWebException;

        protected static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(100);
        protected static readonly TimeSpan DefaultRequestReadWriteTimeout = TimeSpan.FromSeconds(300);

        protected static readonly bool DefaultHandleServiceErrorManually;

        protected static readonly bool DefaultEnableCHystrixSupport;
        protected static readonly bool DefaultEnableCHystrixSupportForIOCPAsync;
        protected static readonly bool DefaultEnableTimeoutForIOCPAsync;

        protected static readonly ConcurrentDictionary<string, ConcurrentDictionary<RequestTimeoutType, TimeSpan>> requestTimeoutSettings =
            new ConcurrentDictionary<string, ConcurrentDictionary<RequestTimeoutType, TimeSpan>>();

        protected static readonly int DefaultCHystrixCommandMaxConcurrentCount;
        protected static readonly ConcurrentDictionary<string, int> DefaultCHystrixCommandMaxConcurrentCountMap = new ConcurrentDictionary<string, int>();

        public static string AppId { get; set; }

        protected internal static readonly string AntServiceStackVersion;

        public static string ServiceRegistryUrl { get; set; }
        public static string ServiceRegistryTestSubEnv { get; set; }

        internal static readonly bool DisableConnectionLeaseTimeout;
        
        static ServiceClientBase()
        {

            AntServiceStackVersion = typeof(ServiceClientBase).Assembly.GetName().Version.ToString();

            // predefined client call format
            IClientCallFormat callFormat = new XmlClientCallFormat();
            CallFormats[callFormat.Format] = callFormat;

            callFormat = new JsonClientCallFormat();
            CallFormats[callFormat.Format] = callFormat;
            
            callFormat = new JsvClientCallFormat();
            CallFormats[callFormat.Format] = callFormat;

            callFormat = new BaijiJsonClientCallFormat();
            CallFormats[callFormat.Format] = callFormat;
            
            callFormat = new BaijiBinaryClientCallFormat();
            CallFormats[callFormat.Format] = callFormat;

            ServiceRegistryTestSubEnv = ClientConfig.Instance.ConfigurationManager.GetPropertyValue(SERVICE_REGISTRY_SUBENV_KEY, "");
            ServiceRegistryTestSubEnv = string.IsNullOrWhiteSpace(ServiceRegistryTestSubEnv) ? null : ServiceRegistryTestSubEnv.Trim().ToLower();

            bool.TryParse(ConfigurationManager.AppSettings[LogErrorWithRequestInfoSettingKey], out DefaultLogErrorWithRequestInfo);
            if (!bool.TryParse(ConfigurationManager.AppSettings[LogWebExceptionAsErrorSettingKey], out DefaultLogWebExceptionAsError))
                DefaultLogWebExceptionAsError = true;
            if (!bool.TryParse(ConfigurationManager.AppSettings[LogCServiceExceptionAsErrorSettingKey], out DefaultLogCServiceExceptionAsError))
                DefaultLogCServiceExceptionAsError = true;
            bool.TryParse(ConfigurationManager.AppSettings[CatIgnoreWarningWebExceptionSettingKey], out DefaultCatIgnoreWarningWebException);

            int timeout = 0;
            if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings[RequestTimeoutSettingKey])
                && int.TryParse(ConfigurationManager.AppSettings[RequestTimeoutSettingKey], out timeout))
                DefaultRequestTimeout = TimeSpan.FromMilliseconds(timeout);
            if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings[RequestReadWriteTimeoutSettingKey])
                && int.TryParse(ConfigurationManager.AppSettings[RequestReadWriteTimeoutSettingKey], out timeout))
                DefaultRequestReadWriteTimeout = TimeSpan.FromMilliseconds(timeout);

            bool.TryParse(ConfigurationManager.AppSettings[HandleServiceErrorManuallySettingKey], out DefaultHandleServiceErrorManually);

            string appId = ConfigUtils.GetNullableAppSetting(AppIdSettingKey);
            if (!string.IsNullOrWhiteSpace(appId))
                AppId = appId.Trim();

            int defaultConnectionLimit;
            int.TryParse(ConfigurationManager.AppSettings[DefaultConnectionLimitSettingKey], out defaultConnectionLimit);
            if (defaultConnectionLimit > 0)
                DefaultConnectionLimit = defaultConnectionLimit;

            int defaultConnectionLeaseTimeout;
            bool tryParseSuccess = int.TryParse(ConfigurationManager.AppSettings[DefaultConnectionLeaseTimeoutSettingKey], out defaultConnectionLeaseTimeout);
            if (!tryParseSuccess)
                defaultConnectionLeaseTimeout = FrameworkDefaultConnectionLeaseTimeout;
            else if (defaultConnectionLeaseTimeout < MinConnectionLeaseTimeout)
                defaultConnectionLeaseTimeout = MinConnectionLeaseTimeout;
            DefaultConnectionLeaseTimeout = defaultConnectionLeaseTimeout;

            bool.TryParse(ConfigurationManager.AppSettings[DisableConnectionLeaseTimeoutSettingKey], out DisableConnectionLeaseTimeout);

            if (!bool.TryParse(ConfigurationManager.AppSettings[EnableCHystrixSupportSettingKey], out DefaultEnableCHystrixSupport))
                DefaultEnableCHystrixSupport = true;

            if (!bool.TryParse(ConfigurationManager.AppSettings[EnableCHystrixSupportForIOCPAsyncSettingKey],
                out DefaultEnableCHystrixSupportForIOCPAsync)) DefaultEnableCHystrixSupportForIOCPAsync = true;

            if (!bool.TryParse(ConfigurationManager.AppSettings[EnableTimeoutForIOCPAsyncSettingKey], out DefaultEnableTimeoutForIOCPAsync))
                DefaultEnableTimeoutForIOCPAsync = true;

            string setting = ConfigurationManager.AppSettings[CHystrixCommandMaxConcurrentCountSettingKey];
            if (!int.TryParse(setting, out DefaultCHystrixCommandMaxConcurrentCount) || DefaultCHystrixCommandMaxConcurrentCount < 0)
                DefaultCHystrixCommandMaxConcurrentCount = 0;

            bool disableAutoCompression;
            if (!bool.TryParse(ConfigurationManager.AppSettings[DisableAutoCompressionSettingKey], out disableAutoCompression))
                disableAutoCompression = false;
            DefaultDisableAutoCompression = disableAutoCompression;

            bool.TryParse(ConfigUtils.GetNullableAppSetting(DeserializeResponseUseMemoryStreamSettingKey), out DefaultDeserializeResponseUseMemoryStream);

            Dictionary<string, string> defaultMaxConcurrentCountMap = ConfigUtils.GetDictionaryFromAppSettingValue(setting);
            foreach (KeyValuePair<string, string> pair in defaultMaxConcurrentCountMap)
            {
                int settingValue;
                int.TryParse(pair.Value, out settingValue);
                if (settingValue > 0)
                    DefaultCHystrixCommandMaxConcurrentCountMap[pair.Key.ToLower()] = settingValue;
            }

            RegisterCustomBadRequestExceptionChecker("SOA2-Client-CServiceException-ValidationError", IsCServiceExceptionValidationError);
        }
        
        /// <summary>
        /// 注册定制调用格式，如x-protobuf
        /// </summary>
        /// <param name="callFormat"></param>
        public static void RegisterCallFormat(IClientCallFormat callFormat) 
        {
            if (CallFormats.ContainsKey(callFormat.Format))
                return;
            CallFormats[callFormat.Format] = callFormat;
        }

        internal static void Reset()
        {
            clientCache.Clear();
            serviceMetadataCache.Clear();
            requestTimeoutSettings.Clear();

            HttpWebRequestFilter = null;
            HttpWebResponseFilter = null;
            RequestFilter = null;
            ResponseFilter = null;
            RequestEndFilter = null;
        }

        /// <summary>
        /// For bad request exception, CHystrix circuit breaker will igore it (think it is a success).
        /// This kind of validation error will not cause chystrix circuit breaker open.
        /// </summary>
        /// <param name="name">the identity for the custom bad request exception checker</param>
        /// <param name="isBadRequestException"></param>
        public static void RegisterCustomBadRequestExceptionChecker(string name, Func<Exception, bool> isBadRequestException)
        {
            if (CHystrixIntegration.HasCHystrix && CHystrixIntegration.HasCustomBadRequestExceptionSupport)
                CHystrixIntegration.RegisterCustomBadRequestExceptionChecker(name, isBadRequestException);
        }

        internal static bool IsCServiceExceptionValidationError(Exception ex)
        {
            try
            {
                CServiceException cex = ex as CServiceException;
                if (cex == null || cex.ResponseErrors == null)
                    return false;

                return cex.ResponseErrors.Count(e => e != null && e.ErrorClassification == ErrorClassificationCodeType.ValidationError) > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of concurrent connections allowed by a System.Net.ServicePoint
        ///     object.
        /// </summary>
        public static int DefaultConnectionLimit
        {
            get { return System.Net.ServicePointManager.DefaultConnectionLimit; }
            set
            {
                if (value > 0)
                {
                    System.Net.ServicePointManager.DefaultConnectionLimit = value;
                }
            }
        }

        internal static int DefaultConnectionLeaseTimeout { get; set; }

        /// <summary>
        /// 获取当前支持的所有调用格式
        /// </summary>
        public static ICollection<string> SupportedFormats
        {
            get { return CallFormats.Keys; }
        }

        /// <summary>
        /// The request filter is called before any request.
        /// This request filter is executed globally.
        /// </summary>
        public static Action<HttpWebRequest> HttpWebRequestFilter { get; set; }

        /// <summary>
        /// The response action is called once the server response is available.
        /// It will allow you to access raw response information. 
        /// This response action is executed globally.
        /// Note that you should NOT consume the response stream as this is handled by ServiceStack
        /// </summary>
        public static Action<HttpWebResponse> HttpWebResponseFilter { get; set; }

        public static Action<string, string, string, object> RequestFilter { get; set; }

        public static Action<string, string, string, object> ResponseFilter { get; set; }

        public static Action<ExecutionContext> RequestEndFilter { get; set; }

        /// <summary>
        /// 默认压缩的开关false
        /// </summary>
        public static bool DefaultDisableAutoCompression { get; set; }

        private static Action<Task> _defaultAsyncInvocationFaultHandler = t =>
        {
            try
            {
                Log.Info("Default async invocation fault handler handled a faulted async call.", t.Exception,
                    new Dictionary<string, string>() { { "ErrorCode", "FXD301042" } });
            }
            catch { }
        };
        public static Action<Task> DefaultAsyncInvocationExceptionHandler
        {
            internal get { return _defaultAsyncInvocationFaultHandler; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Handler cannot be null.");
                _defaultAsyncInvocationFaultHandler = value;
            }
        }

        public static bool DisableAutoAsyncInvocationFaultHandling { get; set; }

        protected internal static Task<T> TryAddFaultHandler<T>(Task<T> task)
        {
            if (!DisableAutoAsyncInvocationFaultHandling)
                task.ContinueWith(DefaultAsyncInvocationExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }

        protected internal static string GetServiceSettingKey(string settingKey, string serviceName, string serviceNamespace)
        {
            return string.Format(settingKey + ServiceUniqueFormat, serviceName, serviceNamespace);
        }

        public TimeSpan? Timeout { get; set; }

        public TimeSpan? ReadWriteTimeout { get; set; }

        private ThreadLocal<TimeSpan?> _currentRequestTimeout = new ThreadLocal<TimeSpan?>();
        public TimeSpan? CurrentRequestTimeout
        {
            get { return _currentRequestTimeout.Value; }
            set { _currentRequestTimeout.Value = value; }
        }

        private ThreadLocal<TimeSpan?> _currentRequestReadWriteTimeout = new ThreadLocal<TimeSpan?>();
        public TimeSpan? CurrentRequestReadWriteTimeout
        {
            get { return _currentRequestReadWriteTimeout.Value; }
            set { _currentRequestReadWriteTimeout.Value = value; }
        }

        protected internal TimeSpan GetDefaultTimeout(RequestTimeoutType settingType)
        {
            TimeSpan timeout;
            switch (settingType)
            {
                case RequestTimeoutType.ReadWriteTimeout:
                    timeout = ReadWriteTimeout ?? DefaultRequestReadWriteTimeout;
                    break;
                default:
                    timeout = Timeout ?? DefaultRequestTimeout;
                    break;
            }

            return timeout;
        }

        protected internal string GenerateOperationTimeoutKey(string operation)
        {
            return (this.GetType().FullName.ToString() +  "." + operation.Trim()).ToLower();
        }

        private void InitOperationTimeoutSettings(string serviceName, string serviceNamespace, RequestTimeoutType settingType, string settingKey)
        {
            string settingValue = GetServiceSettingKey(settingKey, serviceName, serviceNamespace);
            string config = ConfigUtils.GetNullableAppSetting(settingValue);
            if (!string.IsNullOrWhiteSpace(config))
            {
                string[] operationSettings = config.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                TimeSpan defaultTimeout = GetDefaultTimeout(settingType);
                int timeoutMilliseconds;
                TimeSpan timeout;
                foreach (string operationSetting in operationSettings)
                {
                    string[] setting = operationSetting.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (setting.Length != 2)
                        continue;

                    string operation = setting[0].Trim();
                    timeout = defaultTimeout;
                    if (int.TryParse(setting[1].Trim(), out timeoutMilliseconds))
                        timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                    string key = GenerateOperationTimeoutKey(operation);
                    if (!requestTimeoutSettings.ContainsKey(key))
                        requestTimeoutSettings[key] = new ConcurrentDictionary<RequestTimeoutType, TimeSpan>();
                    requestTimeoutSettings[key][settingType] = timeout;
                }
            }
        }

        protected void InitOperationTimeoutSettings(string serviceName, string serviceNamespace)
        {
            InitOperationTimeoutSettings(serviceName, serviceNamespace, RequestTimeoutType.Timeout, RequestTimeoutSettingKey);
            InitOperationTimeoutSettings(serviceName, serviceNamespace, RequestTimeoutType.ReadWriteTimeout, RequestReadWriteTimeoutSettingKey);
        }

        protected internal TimeSpan? GetCurrentRequestTimeout(RequestTimeoutType settingType)
        {
            return settingType == RequestTimeoutType.Timeout ? CurrentRequestTimeout : CurrentRequestReadWriteTimeout;
        }

        protected internal TimeSpan GetOperationTimeout(string operation, RequestTimeoutType settingType)
        {
            TimeSpan? currentRequestTimeout = GetCurrentRequestTimeout(settingType);
            if (currentRequestTimeout.HasValue)
                return currentRequestTimeout.Value;

            string key = GenerateOperationTimeoutKey(operation);
            if (requestTimeoutSettings.ContainsKey(key) && requestTimeoutSettings[key].ContainsKey(settingType))
                return requestTimeoutSettings[key][settingType];

            return GetDefaultTimeout(settingType);
        }

        protected internal void ResetCurrentRequestTimeoutSetting()
        {
            try
            {
                CurrentRequestTimeout = null;
                CurrentRequestReadWriteTimeout = null;
            }
            catch { }
        }

        public void SetOperationTimeout(string operation, RequestTimeoutType settingType, TimeSpan timeout)
        {
            string key = GenerateOperationTimeoutKey(operation);
            if (!requestTimeoutSettings.ContainsKey(key))
                requestTimeoutSettings.TryAdd(key, new ConcurrentDictionary<RequestTimeoutType, TimeSpan>());
            requestTimeoutSettings[key][settingType] = timeout;
        }

        internal static bool IsValidCallFormat(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
                return false;
            return CallFormats.ContainsKey(format);
        }

        internal static bool IsValidJavaCallFormat(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
                return false;
            return string.Equals(format, BaijiJsonClientCallFormat.ContentFormat, StringComparison.OrdinalIgnoreCase) || 
                string.Equals(format, BaijiBinaryClientCallFormat.ContentFormat, StringComparison.OrdinalIgnoreCase);
        }
    }
}
