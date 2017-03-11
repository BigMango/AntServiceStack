using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Web;
using System.IO;
using System.Threading;
using System.Diagnostics;

using AntServiceStack.Common;
using AntServiceStack.Common.Web;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;
using HttpRequestWrapper = AntServiceStack.WebHost.Endpoints.Extensions.HttpRequestWrapper;
using HttpResponseWrapper = AntServiceStack.WebHost.Endpoints.Extensions.HttpResponseWrapper;
using AntServiceStack.Common.Hystrix;
using AntServiceStack.Common.Hystrix.Util;
using AntServiceStack.Common.Hystrix.CircuitBreaker;
using AntServiceStack.Common.Utils;
using AntServiceStack.WebHost.Endpoints;

using Freeway.Logging;

namespace AntServiceStack.WebHost.Endpoints.Support
{
    public class HystrixGlobalStreamHandler : IHttpHandler, IServiceStackHttpHandler
    {
        public const string RestPath = "_soa_hystrix_global_stream";
        public const string NonStreamRestPath = "_soa_hystrix_global";

        const int ClientRetryMilliseconds = 100;

        const string TurbineStrategySemaphore = "SEMAPHORE";
        const string TurbineStrategyThread = "THREAD";

        const string TurbineDataTypeHystrixCommand = "HystrixCommand";
        const string TurbineDataTypeThreadPool = "HystrixThreadPool"; 

        private static readonly ILog Log = LogManager.GetLogger(typeof(HystrixGlobalStreamHandler));

        private string _servicePath;
        private bool _isNonStream;

        public HystrixGlobalStreamHandler(string servicePath)
            : this(servicePath, false)
        {
        }

        public HystrixGlobalStreamHandler(string servicePath, bool isNonStream)
        {
            _servicePath = servicePath;
            _isNonStream = isNonStream;
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            IHttpRequest request = new HttpRequestWrapper(_servicePath, typeof(HystrixGlobalStreamHandler).Name, context.Request);
            IHttpResponse response = new HttpResponseWrapper(context.Response);
            HostContext.InitRequest(request, response);
            ProcessRequest(request, response, typeof(HystrixGlobalStreamHandler).Name);
        }

        public void ProcessRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            try
            {
                if (_isNonStream)
                    ProcessNonStreamRequest(httpReq, httpRes, operationName);
                else
                    ProcessStreamRequest(httpReq, httpRes, operationName);
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to report _hystrix_global_stream.", ex,
                    new Dictionary<string, string>().AddErrorCode("FXD300025"));
            }
        }

        private void ProcessNonStreamRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            httpRes.ContentType = ContentType.Json;
            httpRes.Write(RefinePercentileString(GetHystrixCommandInfoList().ToJson()));
            httpRes.Flush();
        }

        private void ProcessStreamRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            bool isPreflight = WebUtils.EnableCrossDomainSupport(httpReq, httpRes);
            if (isPreflight)
                return;

            httpRes.AddHeader(HttpHeaders.ContentType, ContentType.EventStream + ContentType.Utf8Suffix);
            httpRes.AddHeader(HttpHeaders.CacheControl, "no-cache, no-store, max-age=0, must-revalidate");
            httpRes.AddHeader("Pragma", "no-cache");

            httpRes.Write(string.Format("retry: {0}\n", ClientRetryMilliseconds));

            List<GlobalStreamHystrixCommandInfo> commandInfoList = GetHystrixCommandInfoList();
            foreach (GlobalStreamHystrixCommandInfo commandInfo in commandInfoList)
            {
                httpRes.Write(string.Format("data: {0}\n\n", RefinePercentileString(commandInfo.ToJson())));
                httpRes.Flush();
            }
        }

        private static List<GlobalStreamHystrixCommandInfo> GetHystrixCommandInfoList()
        {
            var result = new List<GlobalStreamHystrixCommandInfo>();
            foreach (ServiceMetadata metadata in EndpointHost.Config.MetadataMap.Values)
            {
                string refinedServiceName = ServiceUtils.RefineServiceName(metadata.ServiceNamespace, metadata.ServiceName);
                foreach (Operation operation in metadata.Operations)
                {
                    HystrixCommandMetrics commandMetrics = operation.HystrixCommand.Metrics;
                    IHystrixCircuitBreaker circuitBreaker = operation.HystrixCommand.CircuitBreaker;
                    HealthCounts healthCounts = commandMetrics.GetHealthCounts();
                    IHystrixCommandProperties commandProperties = commandMetrics.Properties;

                    GlobalStreamHystrixCommandInfo hystrixCommandInfo = new GlobalStreamHystrixCommandInfo()
                    {
                        type = TurbineDataTypeHystrixCommand,
                        name = refinedServiceName + "." + operation.Name.ToLower(),
                        group = refinedServiceName,
                        currentTime = DateTime.Now.ToUnixTimeMs(),
                        isCircuitBreakerOpen = circuitBreaker.IsOpen(),
                        errorPercentage = healthCounts.ErrorPercentage,
                        errorCount = healthCounts.TotalErrorCount,
                        requestCount = healthCounts.TotalRequests,
                        rollingCountExceptionsThrown = healthCounts.TotalExceptionCount,
                        rollingCountFailure = healthCounts.TotalFailureCount,
                        rollingCountSemaphoreRejected = 0,
                        rollingCountShortCircuited = healthCounts.ShortCircuitedCount,
                        rollingCountSuccess = healthCounts.SuccessCount,
                        rollingCountThreadPoolRejected = 0,
                        rollingCountTimeout = healthCounts.TimeoutCount,
                        rollingCountFallbackFailure = 0,
                        rollingCountFallbackSuccess = 0,
                        rollingCountFallbackRejection = 0,
                        latencyExecute = new GlobalStreamPercentileInfo()
                        {
                            P0 = commandMetrics.GetServiceExecutionTimePercentile(0),
                            P25 = commandMetrics.GetServiceExecutionTimePercentile(25),
                            P50 = commandMetrics.GetServiceExecutionTimePercentile(50),
                            P75 = commandMetrics.GetServiceExecutionTimePercentile(75),
                            P90 = commandMetrics.GetServiceExecutionTimePercentile(90),
                            P95 = commandMetrics.GetServiceExecutionTimePercentile(95),
                            P99 = commandMetrics.GetServiceExecutionTimePercentile(99),
                            P99DOT5 = commandMetrics.GetServiceExecutionTimePercentile(99.5),
                            P100 = commandMetrics.GetServiceExecutionTimePercentile(100)
                        },
                        latencyExecute_mean = commandMetrics.GetServiceExecutionTimeMean(),
                        latencyTotal = new GlobalStreamPercentileInfo()
                        {
                            P0 = commandMetrics.GetTotalTimePercentile(0),
                            P25 = commandMetrics.GetTotalTimePercentile(25),
                            P50 = commandMetrics.GetTotalTimePercentile(50),
                            P75 = commandMetrics.GetTotalTimePercentile(75),
                            P90 = commandMetrics.GetTotalTimePercentile(90),
                            P95 = commandMetrics.GetTotalTimePercentile(95),
                            P99 = commandMetrics.GetTotalTimePercentile(99),
                            P99DOT5 = commandMetrics.GetTotalTimePercentile(99.5),
                            P100 = commandMetrics.GetTotalTimePercentile(100)
                        },
                        latencyTotal_mean = commandMetrics.GetTotalTimeMean(),
                        reportingHosts = 1,
                        propertyValue_circuitBreakerEnabled = commandProperties.CircuitBreakerEnabled.Get(),
                        propertyValue_circuitBreakerErrorThresholdPercentage = commandProperties.CircuitBreakerErrorThresholdPercentage.Get(),
                        propertyValue_circuitBreakerForceClosed = commandProperties.CircuitBreakerForceClosed.Get(),
                        propertyValue_circuitBreakerForceOpen = commandProperties.CircuitBreakerForceOpen.Get(),
                        propertyValue_circuitBreakerRequestVolumeThreshold = commandProperties.CircuitBreakerRequestVolumeThreshold.Get(),
                        propertyValue_circuitBreakerSleepWindowInMilliseconds = (int)commandProperties.CircuitBreakerSleepWindow.Get().TotalMilliseconds,
                        propertyValue_executionIsolationSemaphoreMaxConcurrentRequests = 1000,
                        propertyValue_executionIsolationStrategy = TurbineStrategySemaphore,
                        propertyValue_executionIsolationThreadTimeoutInMilliseconds = (int)operation.HystrixCommand.GetExecutionTimeout().TotalMilliseconds,
                        propertyValue_fallbackIsolationSemaphoreMaxConcurrentRequests = 0,
                        propertyValue_metricsRollingStatisticalWindowInMilliseconds = commandProperties.MetricsRollingStatisticalWindowInMilliseconds.Get(),
                        currentConcurrentExecutionCount = commandMetrics.CurrentConcurrentExecutionCount
                    };

                    result.Add(hystrixCommandInfo);
                }
            }

            return result;
        }

        private string RefinePercentileString(string data)
        {
            if (data == null)
                return data;

            Dictionary<string, string> keyToRefined = new Dictionary<string,string>()
            {
                { "\"P0\":", "\"0\":" },
                { "\"P25\":", "\"25\":" },
                { "\"P50\":", "\"50\":" },
                { "\"P75\":", "\"75\":" },
                { "\"P90\":", "\"90\":" },
                { "\"P95\":", "\"95\":" },
                { "\"P99\":", "\"99\":" },
                { "\"P99DOT5\":", "\"99.5\":" },
                { "\"P100\":", "\"100\":" },
            };

            foreach (KeyValuePair<string, string> item in keyToRefined)
            {
                data = data.Replace(item.Key, item.Value);
            }

            return data;
        }
    }

    [DataContract]
    internal class GlobalStreamHystrixCommandInfo
    {
        [DataMember]
        public string type { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string group { get; set; }

        [DataMember]
        public long currentTime { get; set; }

        [DataMember]
        public bool isCircuitBreakerOpen { get; set; }

        [DataMember]
        public long errorPercentage { get; set; }

        [DataMember]
        public long errorCount { get; set; }

        [DataMember]
        public long requestCount { get; set; }

        [DataMember]
        public long rollingCountCollapsedRequests { get; set; }

        [DataMember]
        public long rollingCountExceptionsThrown { get; set; }

        [DataMember]
        public long rollingCountFailure { get; set; }

        [DataMember]
        public long rollingCountFallbackFailure { get; set; }

        [DataMember]
        public long rollingCountFallbackRejection { get; set; }

        [DataMember]
        public long rollingCountFallbackSuccess { get; set; }

        [DataMember]
        public long rollingCountResponsesFromCache { get; set; }

        [DataMember]
        public long rollingCountSemaphoreRejected { get; set; }

        [DataMember]
        public long rollingCountShortCircuited { get; set; }

        [DataMember]
        public long rollingCountSuccess { get; set; }

        [DataMember]
        public long rollingCountThreadPoolRejected { get; set; }

        [DataMember]
        public long rollingCountTimeout { get; set; }

        [DataMember]
        public long currentConcurrentExecutionCount { get; set; }

        [DataMember]
        public long latencyExecute_mean { get; set; }

        [DataMember]
        public GlobalStreamPercentileInfo latencyExecute { get; set; }

        [DataMember]
        public long latencyTotal_mean { get; set; }

        [DataMember]
        public GlobalStreamPercentileInfo latencyTotal { get; set; }

        [DataMember]
        public int propertyValue_circuitBreakerRequestVolumeThreshold { get; set; }

        [DataMember]
        public long propertyValue_circuitBreakerSleepWindowInMilliseconds { get; set; }

        [DataMember]
        public int propertyValue_circuitBreakerErrorThresholdPercentage { get; set; }

        [DataMember]
        public bool propertyValue_circuitBreakerForceOpen { get; set; }

        [DataMember]
        public bool propertyValue_circuitBreakerForceClosed { get; set; }

        [DataMember]
        public bool propertyValue_circuitBreakerEnabled { get; set; }

        [DataMember]
        public string propertyValue_executionIsolationStrategy { get; set; }

        [DataMember]
        public long propertyValue_executionIsolationThreadTimeoutInMilliseconds { get; set; }

        [DataMember]
        public long propertyValue_executionIsolationThreadInterruptOnTimeout { get; set; }

        [DataMember]
        public long propertyValue_executionIsolationThreadPoolKeyOverride { get; set; }

        [DataMember]
        public long propertyValue_executionIsolationSemaphoreMaxConcurrentRequests { get; set; }

        [DataMember]
        public long propertyValue_fallbackIsolationSemaphoreMaxConcurrentRequests { get; set; }

        [DataMember]
        public long propertyValue_metricsRollingStatisticalWindowInMilliseconds { get; set; }

        [DataMember]
        public bool propertyValue_requestCacheEnabled { get; set; }

        [DataMember]
        public bool propertyValue_requestLogEnabled { get; set; }

        [DataMember]
        public int reportingHosts { get; set; }
    }

    [DataContract]
    internal class GlobalStreamPercentileInfo
    {
        [DataMember]
        public long P0 { get; set; }

        [DataMember]
        public long P25 { get; set; }

        [DataMember]
        public long P50 { get; set; }

        [DataMember]
        public long P75 { get; set; }

        [DataMember]
        public long P90 { get; set; }

        [DataMember]
        public long P95 { get; set; }

        [DataMember]
        public long P99 { get; set; }

        [DataMember]
        public long P99DOT5 { get; set; }

        [DataMember]
        public long P100 { get; set; }
    }
}
