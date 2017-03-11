using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Serialization;
using System.Web;
using System.IO;
using System.Threading;
using System.Diagnostics;
using AntServiceStack.Common;
using AntServiceStack.Common.Web;
using AntServiceStack.ServiceHost;
using AntServiceStack.Text;
using Freeway.Logging;
using HttpRequestWrapper = AntServiceStack.WebHost.Endpoints.Extensions.HttpRequestWrapper;
using HttpResponseWrapper = AntServiceStack.WebHost.Endpoints.Extensions.HttpResponseWrapper;
using AntServiceStack.Common.Hystrix;
using AntServiceStack.Common.Hystrix.Util;
using AntServiceStack.Common.Hystrix.CircuitBreaker;

namespace AntServiceStack.WebHost.Endpoints.Support
{
    public class HystrixCommandInfo
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public string Group { get; set; }

        public DateTime CurrentTime { get; set; }

        public bool IsCircuitBreakerOpen { get; set; }

        public int ErrorPercentage { get; set; }

        public long ErrorCount { get; set; }

        public long RequestCount { get; set; }

        public long RollingCountSuccess { get; set; }

        public long RollingCountShortCircuited { get; set; }

        public long RollingCountTimeout { get; set; }

        public long RollingCountThreadPoolRejected { get; set; }

        public long RollingCountFrameworkExceptionThrown { get; set; }

        public long RollingCountServiceExceptionThrown { get; set; }

        public long RollingCountValidationExceptionThrown { get; set; }

        public long CumulativeCountSuccess { get; set; }

        public long CumulativeCountShortCircuited { get; set; }

        public long CumulativeCountTimeout { get; set; }

        public long CumulativeCountThreadPoolRejected { get; set; }

        public long CumulativeCountFrameworkExcetpionThrown { get; set; }

        public long CumulativeCountServiceExceptionThrown { get; set; }

        public long CumulativeCountValidationExceptionThrown { get; set; }

        public long CurrentConcurrentExecutionCount { get; set; }

        public long LatencyExecuteMean { get; set; }

        public PercentileInfo LatencyExecute { get; set; }

        public long LatencyTotalMean { get; set; }

        public PercentileInfo LatencyTotal { get; set; }

        public int PropertyValue_CircuitBreakerRequestVolumeThreshold { get; set; }

        public long PropertyValue_CircuitBreakerSleepWindowInMilliseconds { get; set; }

        public int PropertyValue_CircuitBreakerErrorThresholdPercentage { get; set; }

        public bool PropertyValue_CircuitBreakerForceOpen { get; set; }

        public bool PropertyValue_CircuitBreakerForceClosed { get; set; }

        public bool PropertyValue_CircuitBreakerEnabled { get; set; }

        public long PropertyValue_ExecutionIsolationThreadTimeoutInMilliseconds { get; set; }

        public long PropertyValue_MetricsRollingStatisticalWindowInMilliseconds { get; set; }

        public bool PropertyValue_RequestLogEnabled { get; set; }

        public int ReportingHosts { get; set; }
    }

    public class PercentileInfo
    {
        public double P0 { get; set; }

        public double P25 { get; set; }

        public double P50 { get; set; }

        public double P75 { get; set; }

        public double P90 { get; set; }

        public double P95 { get; set; }

        public double P99 { get; set; }

        public double P99DOT5 { get; set; }

        public double P100 { get; set; }
    }

    public class HystrixInfoHandler : IServiceStackHttpHandler, IHttpHandler
    {
        public const string RestPath = "_hystrix";
        public const string StreamRestPath = "_hystrix_stream";

        /// <summary>
        /// The default time interval between sending new metrics data.
        /// </summary>
        private static readonly int DefaultSendInterval = 2000; // unit is ms

        /// <summary>
        /// The logger instance for this type.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HystrixInfoHandler));

        /// <summary>
        /// Streaming request or non-streaming request
        /// </summary>
        private bool IsStream;

        private string ServicePath;

        public HystrixInfoHandler(bool isStream, string servicePath)
        {
            this.IsStream = isStream;
            this.ServicePath = servicePath;
        }

        public void ProcessRequest(IHttpRequest httpReq, IHttpResponse httpRes, string operationName)
        {
            if (IsStream)
            {
                this.ProcessStreamRequest(httpReq, httpRes);
            }
            else
            {
                this.ProcessNonStreamRequest(httpReq, httpRes);
            }
        }

        private void ProcessNonStreamRequest(IHttpRequest httpReq, IHttpResponse httpRes)
        {
            try
            {
                var response = GetHystrixCommandInfo(httpReq.ServicePath);
                using (var jsConfigScope = JsConfig.BeginScope())
                {
                    jsConfigScope.EmitCamelCaseNames = true;
                    var json = JsonSerializer.SerializeToString(response);
                    httpRes.ContentType = ContentType.Json;
                    httpRes.Write(json);
                }
            }
            catch { }
            finally
            {
                httpRes.Close();
            }
        }

        private void ProcessStreamRequest(IHttpRequest httpReq, IHttpResponse httpRes)
        {
            try
            {
                httpRes.AddHeader("Content-Type", "text/event-stream;charset=UTF-8");
                httpRes.AddHeader("Cache-Control", "no-cache, no-store, max-age=0, must-revalidate");
                httpRes.AddHeader("Pragma", "no-cache");
                // CORS support
                httpRes.AddHeader("Access-Control-Allow-Origin", "*");
                httpRes.AddHeader("Access-Control-Allow-Methods", "GET, POST");
                httpRes.AddHeader("Access-Control-Allow-Headers", "Content-Type");


                var sendInterval = this.GetSendInterval(httpReq);
                var results = GetHystrixCommandInfo(httpReq.ServicePath);

                httpRes.Write(string.Format("retry: {0}\n", sendInterval));

                using (var jsConfigScope = JsConfig.BeginScope())
                {
                    jsConfigScope.EmitCamelCaseNames = true;

                    foreach (var hystrixCommand in results)
                    {
                        var json = JsonSerializer.SerializeToString(hystrixCommand);
                        httpRes.Write(string.Format("data: {0}\n\n", json));
                    }

                    httpRes.Flush();
                }

            }
            catch { }
            finally
            {
                httpRes.Close();
            }
        }

        /*
        private int GetLastEventId(IHttpRequest httpReq)
        {
            int lastEventId = DefaultLastEventId;
            var idString = httpReq.QueryString["last-event-id"];
            if (idString == null)
                idString = httpReq.Headers["last-event-id"];

            if (idString != null)
            {
                int idInt = 0;
                if (int.TryParse(idString, out idInt))
                {
                    if (idInt > 0 && idInt + MaxStreamingCount < int.MaxValue)
                        lastEventId = idInt;
                }
            }

            return lastEventId;
        }
        */
        
        /// <summary>
        /// Extracts the sending interval from the HTTP request.
        /// </summary>
        /// <returns>The time interval between sending new metrics data.</returns>
        private int GetSendInterval(IHttpRequest httpReq)
        {
            int sendInterval = DefaultSendInterval;
            if (httpReq.QueryString["delay"] != null)
            {
                var delay = httpReq.QueryString["delay"];
                int streamDelayInMilliseconds = 0;
                if (int.TryParse(delay, out streamDelayInMilliseconds))
                {
                    // Limit streaming frequency to avoid too much overhead
                    if (streamDelayInMilliseconds > DefaultSendInterval)
                    {
                        sendInterval = streamDelayInMilliseconds;
                    }
                }
                else
                {
                    Logger.Warn(string.Format("Invalid delay parameter in request: '{0}'", delay), 
                        new Dictionary<string, string>() 
                        { 
                            { "ErrorCode", "FXD300069" } 
                        });
                }
            }

            return sendInterval;
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            IHttpRequest request = new HttpRequestWrapper(ServicePath, typeof(HystrixInfoHandler).Name, context.Request);
            IHttpResponse response = new HttpResponseWrapper(context.Response);
            HostContext.InitRequest(request, response);
            ProcessRequest(request, response, typeof(HystrixInfoHandler).Name);
        }

        public static List<HystrixCommandInfo> GetHystrixCommandInfo(string servicePath)
        {
            var result = new List<HystrixCommandInfo>();
            ServiceMetadata metadata = EndpointHost.Config.MetadataMap[servicePath];
            foreach (Operation operation in metadata.Operations)
            {
                HystrixCommandMetrics commandMetrics = operation.HystrixCommand.Metrics;
                IHystrixCircuitBreaker circuitBreaker = operation.HystrixCommand.CircuitBreaker;
                HealthCounts healthCounts = commandMetrics.GetHealthCounts();
                IHystrixCommandProperties commandProperties = commandMetrics.Properties;

                var commandInfo = new HystrixCommandInfo
                {
                    Type = "HystrixCommand",
                    Name = commandMetrics.OperationName,
                    Group = commandMetrics.FullServiceName,
                    CurrentTime = DateTime.Now,
                    IsCircuitBreakerOpen = (circuitBreaker == null ? false : circuitBreaker.IsOpen()),
                    ErrorPercentage = healthCounts.ErrorPercentage,
                    ErrorCount = healthCounts.TotalErrorCount,
                    RequestCount = healthCounts.TotalRequests,
                    RollingCountSuccess = commandMetrics.GetRollingCount(HystrixRollingNumberEvent.Success),
                    RollingCountShortCircuited = commandMetrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited),
                    RollingCountTimeout = commandMetrics.GetRollingCount(HystrixRollingNumberEvent.Timeout),
                    RollingCountThreadPoolRejected = commandMetrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected),
                    RollingCountFrameworkExceptionThrown = commandMetrics.GetRollingCount(HystrixRollingNumberEvent.FrameworkExceptionThrown),
                    RollingCountServiceExceptionThrown = commandMetrics.GetRollingCount(HystrixRollingNumberEvent.ServiceExceptionThrown),
                    RollingCountValidationExceptionThrown = commandMetrics.GetRollingCount(HystrixRollingNumberEvent.ValidationExceptionThrown),
                    CumulativeCountSuccess = commandMetrics.GetCumulativeCount(HystrixRollingNumberEvent.Success),
                    CumulativeCountShortCircuited = commandMetrics.GetCumulativeCount(HystrixRollingNumberEvent.ShortCircuited),
                    CumulativeCountTimeout = commandMetrics.GetCumulativeCount(HystrixRollingNumberEvent.Timeout),
                    CumulativeCountThreadPoolRejected = commandMetrics.GetCumulativeCount(HystrixRollingNumberEvent.ThreadPoolRejected),
                    CumulativeCountFrameworkExcetpionThrown = commandMetrics.GetCumulativeCount(HystrixRollingNumberEvent.FrameworkExceptionThrown),
                    CumulativeCountServiceExceptionThrown = commandMetrics.GetCumulativeCount(HystrixRollingNumberEvent.ServiceExceptionThrown),
                    CumulativeCountValidationExceptionThrown = commandMetrics.GetCumulativeCount(HystrixRollingNumberEvent.ValidationExceptionThrown),
                    CurrentConcurrentExecutionCount = commandMetrics.CurrentConcurrentExecutionCount,
                    LatencyExecuteMean = commandMetrics.GetServiceExecutionTimeMean(),
                    LatencyExecute = new PercentileInfo
                    {
                        P0 = commandMetrics.GetServiceExecutionTimePercentile(0),
                        P25 = commandMetrics.GetServiceExecutionTimePercentile(25),
                        P50 = commandMetrics.GetServiceExecutionTimePercentile(50),
                        P75 = commandMetrics.GetServiceExecutionTimePercentile(75),
                        P90 = commandMetrics.GetServiceExecutionTimePercentile(90),
                        P95 = commandMetrics.GetServiceExecutionTimePercentile(95),
                        P99 = commandMetrics.GetServiceExecutionTimePercentile(99),
                        P99DOT5 = commandMetrics.GetServiceExecutionTimePercentile(99.5),
                        P100 = commandMetrics.GetServiceExecutionTimePercentile(100),
                    },
                    LatencyTotalMean = commandMetrics.GetTotalTimeMean(),
                    LatencyTotal = new PercentileInfo
                    {
                        P0 = commandMetrics.GetTotalTimePercentile(0),
                        P25 = commandMetrics.GetTotalTimePercentile(25),
                        P50 = commandMetrics.GetTotalTimePercentile(50),
                        P75 = commandMetrics.GetTotalTimePercentile(75),
                        P90 = commandMetrics.GetTotalTimePercentile(90),
                        P95 = commandMetrics.GetTotalTimePercentile(95),
                        P99 = commandMetrics.GetTotalTimePercentile(99),
                        P99DOT5 = commandMetrics.GetTotalTimePercentile(99.5),
                        P100 = commandMetrics.GetTotalTimePercentile(100),
                    },
                    PropertyValue_CircuitBreakerRequestVolumeThreshold = commandProperties.CircuitBreakerRequestVolumeThreshold.Get(),
                    PropertyValue_CircuitBreakerSleepWindowInMilliseconds = (long)commandProperties.CircuitBreakerSleepWindow.Get().TotalMilliseconds,
                    PropertyValue_CircuitBreakerErrorThresholdPercentage = commandProperties.CircuitBreakerErrorThresholdPercentage.Get(),
                    PropertyValue_CircuitBreakerForceOpen = commandProperties.CircuitBreakerForceOpen.Get(),
                    PropertyValue_CircuitBreakerForceClosed = commandProperties.CircuitBreakerForceClosed.Get(),
                    PropertyValue_CircuitBreakerEnabled = commandProperties.CircuitBreakerEnabled.Get(),
                    PropertyValue_ExecutionIsolationThreadTimeoutInMilliseconds = (long)operation.HystrixCommand.GetExecutionTimeout().TotalMilliseconds,
                    PropertyValue_MetricsRollingStatisticalWindowInMilliseconds = commandProperties.MetricsRollingStatisticalWindowInMilliseconds.Get(),
                    PropertyValue_RequestLogEnabled = commandProperties.RequestLogEnabled.Get(),
                    ReportingHosts = 1,
                };

                result.Add(commandInfo);
            }

            return result;
        }
    }
}
