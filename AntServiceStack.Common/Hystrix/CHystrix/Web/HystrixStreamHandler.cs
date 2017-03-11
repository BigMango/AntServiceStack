namespace CHystrix.Web
{
    using CHystrix;
    using CHystrix.Threading;
    using CHystrix.Utils;
    using CHystrix.Utils.Extensions;
    using CHystrix.Utils.Web;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Routing;

    internal class HystrixStreamHandler : IHttpHandler, IRouteHandler
    {
        private const int ClientRetryMilliseconds = 100;
        public const string OperationName = "_hystrix_stream";
        private const string TurbineDataTypeHystrixCommand = "HystrixCommand";
        private const string TurbineDataTypeThreadPool = "HystrixThreadPool";
        private const string TurbineStrategySemaphore = "SEMAPHORE";
        private const string TurbineStrategyThread = "THREAD";

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return this;
        }

        public static List<HystrixCommandInfo> GetHystrixCommandInfoList()
        {
            List<HystrixCommandInfo> list = new List<HystrixCommandInfo>();
            foreach (CommandComponents components in HystrixCommandBase.CommandComponentsCollection.Values.ToArray<CommandComponents>())
            {
                CommandExecutionHealthSnapshot executionHealthSnapshot = components.Metrics.GetExecutionHealthSnapshot();
                Dictionary<CommandExecutionEventEnum, int> executionEventDistribution = components.Metrics.GetExecutionEventDistribution();
                HystrixCommandInfo info2 = new HystrixCommandInfo {
                    type = "HystrixCommand",
                    name = components.CommandInfo.Key,
                    group = (components.CommandInfo.InstanceKey == null) ? components.CommandInfo.GroupKey : components.CommandInfo.CommandKey,
                    currentTime = CommonUtils.CurrentUnixTimeInMilliseconds,
                    isCircuitBreakerOpen = components.CircuitBreaker.IsOpen(),
                    errorPercentage = executionHealthSnapshot.ErrorPercentage,
                    errorCount = ((IEnumerable<int>) (from p in executionEventDistribution
                        where CommonUtils.CoreFailedCommandExecutionEvents.Contains<CommandExecutionEventEnum>(p.Key)
                        select p.Value)).Sum(),
                    requestCount = ((IEnumerable<int>) (from p in executionEventDistribution
                        where CommonUtils.CoreCommandExecutionEvents.Contains<CommandExecutionEventEnum>(p.Key)
                        select p.Value)).Sum(),
                    rollingCountExceptionsThrown = (long) executionEventDistribution[CommandExecutionEventEnum.ExceptionThrown],
                    rollingCountFailure = (long) executionEventDistribution[CommandExecutionEventEnum.Failed],
                    rollingCountSemaphoreRejected = (components.IsolationMode == IsolationModeEnum.SemaphoreIsolation) ? ((long) executionEventDistribution[CommandExecutionEventEnum.Rejected]) : ((long) 0),
                    rollingCountShortCircuited = (long) executionEventDistribution[CommandExecutionEventEnum.ShortCircuited],
                    rollingCountSuccess = (long) executionEventDistribution[CommandExecutionEventEnum.Success],
                    rollingCountThreadPoolRejected = (components.IsolationMode == IsolationModeEnum.ThreadIsolation) ? ((long) executionEventDistribution[CommandExecutionEventEnum.Rejected]) : ((long) 0),
                    rollingCountTimeout = (long) executionEventDistribution[CommandExecutionEventEnum.Timeout],
                    rollingCountFallbackFailure = (long) executionEventDistribution[CommandExecutionEventEnum.FallbackFailed],
                    rollingCountFallbackSuccess = (long) executionEventDistribution[CommandExecutionEventEnum.FallbackSuccess],
                    rollingCountFallbackRejection = (long) executionEventDistribution[CommandExecutionEventEnum.FallbackRejected]
                };
                PercentileInfo info3 = new PercentileInfo {
                    P0 = components.Metrics.GetExecutionLatencyPencentile(0.0),
                    P25 = components.Metrics.GetExecutionLatencyPencentile(25.0),
                    P50 = components.Metrics.GetExecutionLatencyPencentile(50.0),
                    P75 = components.Metrics.GetExecutionLatencyPencentile(75.0),
                    P90 = components.Metrics.GetExecutionLatencyPencentile(90.0),
                    P95 = components.Metrics.GetExecutionLatencyPencentile(95.0),
                    P99 = components.Metrics.GetExecutionLatencyPencentile(99.0),
                    P99DOT5 = components.Metrics.GetExecutionLatencyPencentile(99.5),
                    P100 = components.Metrics.GetExecutionLatencyPencentile(100.0)
                };
                info2.latencyExecute = info3;
                info2.latencyExecute_mean = components.Metrics.GetAverageExecutionLatency();
                PercentileInfo info4 = new PercentileInfo {
                    P0 = components.Metrics.GetTotalExecutionLatencyPencentile(0.0),
                    P25 = components.Metrics.GetTotalExecutionLatencyPencentile(25.0),
                    P50 = components.Metrics.GetTotalExecutionLatencyPencentile(50.0),
                    P75 = components.Metrics.GetTotalExecutionLatencyPencentile(75.0),
                    P90 = components.Metrics.GetTotalExecutionLatencyPencentile(90.0),
                    P95 = components.Metrics.GetTotalExecutionLatencyPencentile(95.0),
                    P99 = components.Metrics.GetTotalExecutionLatencyPencentile(99.0),
                    P99DOT5 = components.Metrics.GetTotalExecutionLatencyPencentile(99.5),
                    P100 = components.Metrics.GetTotalExecutionLatencyPencentile(100.0)
                };
                info2.latencyTotal = info4;
                info2.latencyTotal_mean = components.Metrics.GetAverageTotalExecutionLatency();
                info2.reportingHosts = 1;
                info2.propertyValue_circuitBreakerEnabled = components.ConfigSet.CircuitBreakerEnabled;
                info2.propertyValue_circuitBreakerErrorThresholdPercentage = components.ConfigSet.CircuitBreakerErrorThresholdPercentage;
                info2.propertyValue_circuitBreakerForceClosed = components.ConfigSet.CircuitBreakerForceClosed;
                info2.propertyValue_circuitBreakerForceOpen = components.ConfigSet.CircuitBreakerForceOpen;
                info2.propertyValue_circuitBreakerRequestVolumeThreshold = components.ConfigSet.CircuitBreakerRequestCountThreshold;
                info2.propertyValue_circuitBreakerSleepWindowInMilliseconds = components.ConfigSet.CircuitBreakerSleepWindowInMilliseconds;
                info2.propertyValue_executionIsolationSemaphoreMaxConcurrentRequests = components.ConfigSet.CommandMaxConcurrentCount;
                info2.propertyValue_executionIsolationStrategy = (components.IsolationMode == IsolationModeEnum.SemaphoreIsolation) ? "SEMAPHORE" : "THREAD";
                info2.propertyValue_executionIsolationThreadTimeoutInMilliseconds = components.ConfigSet.CommandTimeoutInMilliseconds;
                info2.propertyValue_fallbackIsolationSemaphoreMaxConcurrentRequests = components.ConfigSet.FallbackMaxConcurrentCount;
                info2.propertyValue_metricsRollingStatisticalWindowInMilliseconds = components.ConfigSet.MetricsRollingStatisticalWindowInMilliseconds;
                info2.currentConcurrentExecutionCount = components.Metrics.CurrentConcurrentExecutionCount;
                HystrixCommandInfo item = info2;
                list.Add(item);
            }
            return list;
        }

        public static List<HystrixThreadPoolInfo> GetHystrixThreadPoolList()
        {
            List<HystrixThreadPoolInfo> list = new List<HystrixThreadPoolInfo>();
            foreach (string str in CThreadPoolFactory.AllPools.Keys.ToArray<string>())
            {
                CThreadPool pool;
                CommandComponents components;
                if (CThreadPoolFactory.AllPools.TryGetValue(str, out pool) && HystrixCommandBase.CommandComponentsCollection.TryGetValue(str, out components))
                {
                    ICommandConfigSet configSet = components.ConfigSet;
                    HystrixThreadPoolInfo item = new HystrixThreadPoolInfo {
                        type = "HystrixThreadPool",
                        name = str,
                        currentActiveCount = pool.NowRunningWorkCount,
                        currentCorePoolSize = pool.PoolThreadCount,
                        currentQueueSize = pool.NowWaitingWorkCount,
                        currentLargestPoolSize = pool.LargestPoolSize,
                        currentMaximumPoolSize = pool.MaxConcurrentCount,
                        currentCompletedTaskCount = pool.FinishedWorkCount,
                        currentPoolSize = pool.CurrentPoolSize,
                        currentTaskCount = pool.CurrentTaskCount,
                        currentTime = CommonUtils.CurrentUnixTimeInMilliseconds,
                        propertyValue_metricsRollingStatisticalWindowInMilliseconds = configSet.MetricsRollingStatisticalWindowInMilliseconds,
                        propertyValue_queueSizeRejectionThreshold = (configSet.CommandMaxConcurrentCount * configSet.MaxAsyncCommandExceedPercentage) / 100,
                        reportingHosts = 1,
                        rollingCountThreadsExecuted = 0,
                        rollingMaxActiveThreads = 0
                    };
                    list.Add(item);
                }
            }
            return list;
        }

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                if (!context.EnableCrossDomainSupport())
                {
                    context.Response.AddHeader("Content-Type", "text/event-stream; charset=utf-8");
                    context.Response.AddHeader("Cache-Control", "no-cache, no-store, max-age=0, must-revalidate");
                    context.Response.AddHeader("Pragma", "no-cache");
                    context.Response.Write(string.Format("retry: {0}\n", 100));
                    foreach (HystrixCommandInfo info in GetHystrixCommandInfoList())
                    {
                        context.Response.Write(string.Format("data: {0}\n\n", this.RefinePercentileString(info.ToJson())));
                        context.Response.Flush();
                    }
                    foreach (HystrixThreadPoolInfo info2 in GetHystrixThreadPoolList())
                    {
                        context.Response.Write(string.Format("data: {0}\n\n", info2.ToJson()));
                        context.Response.Flush();
                    }
                }
            }
            catch (Exception exception)
            {
                CommonUtils.Log.Log(LogLevelEnum.Warning, "Failed to report Hystrix.stream metrics.", exception, new Dictionary<string, string>().AddLogTagData("FXD303027"));
            }
        }

        private string RefinePercentileString(string data)
        {
            if (data != null)
            {
                Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
                dictionary2.Add("\"P0\":", "\"0\":");
                dictionary2.Add("\"P25\":", "\"25\":");
                dictionary2.Add("\"P50\":", "\"50\":");
                dictionary2.Add("\"P75\":", "\"75\":");
                dictionary2.Add("\"P90\":", "\"90\":");
                dictionary2.Add("\"P95\":", "\"95\":");
                dictionary2.Add("\"P99\":", "\"99\":");
                dictionary2.Add("\"P99DOT5\":", "\"99.5\":");
                dictionary2.Add("\"P100\":", "\"100\":");
                Dictionary<string, string> dictionary = dictionary2;
                foreach (KeyValuePair<string, string> pair in dictionary)
                {
                    data = data.Replace(pair.Key, pair.Value);
                }
            }
            return data;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}

