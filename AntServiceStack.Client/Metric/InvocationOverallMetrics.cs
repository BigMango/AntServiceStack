//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using System.Threading;
//using System.Collections.Concurrent;
//using Freeway.Metrics;
//using AntServiceStack.Common.Utils;
//using AntServiceStack.ServiceClient;
//using AntServiceStack.Common.Hystrix.Util;

//namespace AntServiceStack.Client.Metric
//{
//    public class InvocationOverallMetrics
//    {
//        public const string OverallMetricsNamePrefixRequest = "soa.request.";
//        public const string OverallServiceNamePrefix = "soa.ctrip.com.";
//        public const string OverallMetricsNameRequestCount = OverallMetricsNamePrefixRequest + "count";
//        public const string OverallMetricsNamePrefixRequestLatency = OverallMetricsNamePrefixRequest + "latency.";
//        public const string OverallMetricsNameRequestLatencyDistribution = OverallMetricsNamePrefixRequestLatency + "distribution";
//        public const string OverallMetricsNameRequestLatency = OverallMetricsNamePrefixRequest + "latency";
//        public const string OverallMetricsNameExceptionCount = "soa.exception.count";

//        public const string OverallMetricsTagNameDistribution = "distribution";
//        public const string OverallMetricsTagNameOperation = "operation";
//        public const string OverallMetricsTagNameWebService = "webservice";
//        public const string OverallMetricsTagNameExceptionType = "exceptiontype";
//        public const string OverallMetricsTagNameExceptionName = "exceptionname";
//        public const string OverallMetricsTagNameFrameworkVersion = "frameworkversion";
//        public const string OverallMetricsTagNameConnectionMode = "connectionmode";
//        public const string OverallMetricsTagNameSetFeatureType = "SetFeatureType";

//        private const long OverallMetricsSendingInterval = 60 * 1000;

//        private IMetric _logger;
//        private string _serviceName;
//        private string _connectionMode;
//        private string _frameworkVersion;
//        private ConcurrentDictionary<string, InvocationMetrics> _metrics;

//        public InvocationOverallMetrics(
//            string serviceNamespace,
//            string serviceName,
//            string serviceStackVersion,
//            string codeGeneratorVersion,
//            ConnectionMode connectionMode,
//            bool isSLBService,
//            IMetric logger,
//            long metricsSendingInterval,
//            ConcurrentDictionary<string, InvocationMetrics> metrics)
//        {
//            _serviceName = RefineServiceName(serviceNamespace, serviceName);
//            _connectionMode = connectionMode.ToString();
//            _logger = logger;
//            _metrics = metrics;
//            _frameworkVersion = GenerateFrameworkVersion(serviceStackVersion, codeGeneratorVersion, isSLBService);
//        }

//        private string RefineServiceName(string serviceNamespace, string serviceName)
//        {
//            return (ServiceUtils.ConvertNamespaceToMetricPrefix(serviceNamespace) + "." + serviceName).ToLower()
//                .Replace(OverallServiceNamePrefix, string.Empty);
//        }

//        private string GenerateFrameworkVersion(string serviceStackVersion, string codeGeneratorVersion, bool isSLBService)
//        {
//            string versionFormat = isSLBService ? "SS-{0} BJCG-{1}" : "SS-{0} CG-{1}";
//            return string.Format(versionFormat, serviceStackVersion, codeGeneratorVersion);
//        }

//        public void SendMetrics()
//        {
//            Func<int, long, long> caculateAvg = (count, sum) =>
//            {
//                long avg = 0;
//                if (count > 0)
//                    avg = (long)Math.Round((double)sum / count);
//                return avg;
//            };

//            var tagMap = new Dictionary<string, string>();
//            tagMap[OverallMetricsTagNameWebService] = _serviceName;
//            tagMap[OverallMetricsTagNameFrameworkVersion] = _frameworkVersion;
//            tagMap[OverallMetricsTagNameConnectionMode] = _connectionMode;
//            var now = DateTime.Now;

//            Action<InvocationMetrics, int, int?, string> logDistribution = (m, s, e, t) =>
//            {
//                int distributionCount = m.GetInvocationCountInTimeRange(s, e);
//                if (distributionCount <= 0)
//                    return;

//                tagMap[OverallMetricsTagNameDistribution] = t;
//                _logger.log(OverallMetricsNameRequestLatencyDistribution, distributionCount, tagMap, now);
//            };

//            List<string> operations = _metrics.Keys.ToList();
//            foreach (string operation in operations)
//            {
//                tagMap[OverallMetricsTagNameOperation] = _serviceName + "." + operation.ToLower();
//                InvocationMetrics metrics = _metrics[operation];

//                // request latency distribution
//                logDistribution(metrics, 0, 10, "0 ~ 10ms");
//                logDistribution(metrics, 10, 50, "10 ~ 50ms");
//                logDistribution(metrics, 50, 200, "50 ~ 200ms");
//                logDistribution(metrics, 200, 500, "200 ~ 500ms");
//                logDistribution(metrics, 500, 1000, "500ms ~ 1s");
//                logDistribution(metrics, 1000, 5 * 1000, "1 ~ 5s");
//                logDistribution(metrics, 5 * 1000, 10 * 1000, "5 ~ 10s");
//                logDistribution(metrics, 10 * 1000, 30 * 1000, "10 ~ 30s");
//                logDistribution(metrics, 30 * 1000, 100 * 1000, "30 ~ 100s");
//                logDistribution(metrics, 100 * 1000, null, ">= 100s");
//                if (tagMap.ContainsKey(OverallMetricsTagNameDistribution))
//                    tagMap.Remove(OverallMetricsTagNameDistribution);

//                // request count
//                _logger.log(OverallMetricsNameRequestCount, metrics.GetInvocationCount(), tagMap, now);

//                // latency
//                int count;
//                long sum, min, max;
//                metrics.GetInvocationTimeMetricsData(out count, out sum, out min, out max);
//                tagMap[OverallMetricsTagNameSetFeatureType] = "count";
//                _logger.log(OverallMetricsNameRequestLatency, count, tagMap, now);
//                tagMap[OverallMetricsTagNameSetFeatureType] = "sum";
//                _logger.log(OverallMetricsNameRequestLatency, sum, tagMap, now);
//                tagMap[OverallMetricsTagNameSetFeatureType] = "min";
//                _logger.log(OverallMetricsNameRequestLatency, min, tagMap, now);
//                tagMap[OverallMetricsTagNameSetFeatureType] = "max";
//                _logger.log(OverallMetricsNameRequestLatency, max, tagMap, now);
//                tagMap.Remove(OverallMetricsTagNameSetFeatureType);

//                // exception
//                ConcurrentDictionary<string, ConcurrentDictionary<string, LongAdder>> exceptionCounter = metrics.GetInvocationExceptionCounter();
//                List<string> exceptionTypes = exceptionCounter.Keys.ToList();
//                foreach (string exceptionType in exceptionTypes)
//                {
//                    tagMap[OverallMetricsTagNameExceptionType] = exceptionType;
//                    List<string> exceptionNames = exceptionCounter[exceptionType].Keys.ToList();
//                    foreach (string exceptionName in exceptionNames)
//                    {
//                        long exceptionCount = exceptionCounter[exceptionType][exceptionName].Sum();
//                        if (exceptionCount <= 0)
//                            continue;

//                        tagMap[OverallMetricsTagNameExceptionName] = exceptionName;
//                        _logger.log(OverallMetricsNameExceptionCount, exceptionCount, tagMap, now);
//                    }
//                }
//                if (tagMap.ContainsKey(OverallMetricsTagNameExceptionType))
//                    tagMap.Remove(OverallMetricsTagNameExceptionType);
//                if (tagMap.ContainsKey(OverallMetricsTagNameExceptionName))
//                    tagMap.Remove(OverallMetricsTagNameExceptionName);

//                // Reset
//                metrics.ResetCounters();
//            }
//        }
//    }
//}
