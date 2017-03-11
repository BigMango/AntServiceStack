//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using AntServiceStack.Common.Types;
//using System.Reflection;
//using System.Linq.Expressions;
//using Com.Ctrip.Soa.Caravan.Metric;
//using AntServiceStack.Common.Utils;
//using System.Diagnostics;
//using AntServiceStack.ServiceHost;
//using Freeway.Logging;

//namespace AntServiceStack.WebHost.Endpoints.Registry
//{
//    internal class HealthCheckContext
//    {
//        private static ILog _logger = LogManager.GetLogger(typeof(HealthCheckContext));

//        public IAuditMetric LatencyMetric { get; private set; }
//        public IEventMetric EventMetric { get; private set; }
//        public ServiceMetadata ServiceMetadata { get; private set; }

//        private Func<object, object> _executor;
//        private Dictionary<string, string> _logTags;
//        private readonly bool initialized;

//        public HealthCheckContext(ServiceMetadata serviceMetadata)
//        {
//            ServiceMetadata = serviceMetadata;
//            try
//            {
//                InitExecutor();
//                InitMetric();
//                initialized = true;
//            }
//            catch (Exception ex)
//            {
//                initialized = false;
//                _logger.Error("Failed to initialize HealthCheckContext.", ex, GetLogTags());
//            }
//        }

//        public bool IsHealthy
//        {
//            get
//            {
//                if (!initialized)
//                    return true;

//                Stopwatch watch = new Stopwatch();
//                watch.Start();
//                bool success = CheckHealth();
//                watch.Stop();
//                LatencyMetric.AddValue(watch.ElapsedMilliseconds);
//                EventMetric.AddEvent(success ? "success" : "failure");
//                return success;
//            }
//        }

//        private bool CheckHealth()
//        {
//            try
//            {
//                var response = (CheckHealthResponseType)_executor(new CheckHealthRequestType());
//                if (response == null)
//                {
//                    _logger.Error("CheckHealth returned null.", GetLogTags());
//                    return true;
//                }
//                if (response.ResponseStatus == null || response.ResponseStatus.Ack != AckCodeType.Failure)
//                    return true;

//                _logger.Warn(CreateErrorMessage(response.ResponseStatus.Errors), GetLogTags());
//                return false;
//            }
//            catch (Exception ex)
//            {
//                _logger.Error("Error occurred in CheckHealth.", ex, GetLogTags());
//                return true;
//            }
//        }

//        private string CreateErrorMessage(List<ErrorDataType> errors)
//        {
//            StringBuilder builder = new StringBuilder("CheckHealth returned failure.");
//            if (errors == null || errors.Count == 0)
//                return builder.ToString();

//            bool hasContent;
//            foreach (var error in errors)
//            {
//                if (error == null)
//                    continue;

//                hasContent = false;
//                if (!string.IsNullOrWhiteSpace(error.ErrorCode))
//                {
//                    builder.AppendFormat(" ErrorCode: {0}", error.ErrorCode);
//                    hasContent = true;
//                }
//                if (!string.IsNullOrWhiteSpace(error.Message))
//                {
//                    if (hasContent)
//                        builder.Append(", ");
//                    builder.AppendFormat("Message: {0}", error.Message);
//                    hasContent = true;
//                }
//                if (!string.IsNullOrWhiteSpace(error.StackTrace))
//                {
//                    if (hasContent)
//                        builder.Append(", ");
//                    builder.AppendFormat("StackTrace: {0}", error.StackTrace);
//                    hasContent = true;
//                }
//                if (hasContent)
//                    builder.AppendLine(".");
//            }
//            return builder.ToString();
//        }

//        private Dictionary<string, string> GetLogTags()
//        {
//            if (_logTags != null)
//                return _logTags;

//            _logTags = new Dictionary<string, string>();
//            _logTags["Service"] = ServiceMetadata.RefinedFullServiceName;
//            _logTags["ServiceName"] = ServiceMetadata.ServiceName;
//            _logTags["ServiceNamespace"] = ServiceMetadata.ServiceNamespace;
//            return _logTags;
//        }

//        private void InitExecutor()
//        {
//            var serviceType = ServiceMetadata.ServiceTypes[0];
//            MethodInfo checkHealthMethod = serviceType.GetMethod(ServiceUtils.CheckHealthOperationName);

//            var serviceParameter = Expression.Parameter(typeof(object), "service");
//            var requestParameter = Expression.Parameter(typeof(object), "request");
//            Expression callMethod = Expression.Call(
//                Expression.Convert(serviceParameter, serviceType),
//                checkHealthMethod,
//                new Expression[] { Expression.Convert(requestParameter, typeof(CheckHealthRequestType)) });
//            Func<object, object, object> checkHealth =
//                Expression.Lambda<Func<object, object, object>>(callMethod, serviceParameter, requestParameter).Compile();

//            Func<Type, object> createServiceInstance = Expression.Lambda<Func<Type, object>>
//                (
//                    Expression.New(serviceType),
//                    Expression.Parameter(typeof(Type), "serviceType")
//                ).Compile();

//            _executor = request => checkHealth(createServiceInstance(serviceType), request);
//        }

//        private void InitMetric()
//        {
//            string latencyMetricName = "soa.service.checkhealth.latency";
//            string latencyDistributionMetricName = "soa.service.checkhealth.latency.distribution";
//            var latencyMetricMetadata = new Dictionary<string, string>();
//            latencyMetricMetadata["metric_name_audit"] = latencyMetricName;
//            latencyMetricMetadata["metric_name_distribution"] = latencyDistributionMetricName;
//            latencyMetricMetadata["webservice"] = ServiceMetadata.RefinedFullServiceName;
//            MetricConfig latencyMetricConfig = new MetricConfig(latencyMetricMetadata);
//            LatencyMetric = ArtemisServiceConstants.AuditMetricManager.GetMetric(GetMetricId(latencyMetricName), latencyMetricConfig);

//            string eventDistributionMetricName = "soa.service.checkhealth.event.distribution";
//            var eventMetricMetadata = new Dictionary<string, string>();
//            eventMetricMetadata["metric_name_distribution"] = eventDistributionMetricName;
//            eventMetricMetadata["webservice"] = ServiceMetadata.RefinedFullServiceName;
//            MetricConfig eventMetricConfig = new MetricConfig(eventMetricMetadata);
//            EventMetric = ArtemisServiceConstants.EventMetricManager.GetMetric(GetMetricId(eventDistributionMetricName), eventMetricConfig);
//        }

//        private string GetMetricId(string metricName)
//        {
//            return string.Format("{0}.{1}", ServiceMetadata.RefinedFullServiceName, metricName);
//        }
//    }
//}
