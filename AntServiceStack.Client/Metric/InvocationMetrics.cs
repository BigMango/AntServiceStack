//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Threading;
//using System.Net;
//using AntServiceStack.Common.Hystrix;
//using AntServiceStack.Common.Hystrix.Strategy;
//using AntServiceStack.Common.Hystrix.Util;
//using Freeway.Logging;
//using AntServiceStack.Common.Types;
//using AntServiceStack.ServiceClient;

//namespace AntServiceStack.Client.Metric
//{
//    public class InvocationMetrics
//    {
//        private readonly IInvocationMetricsProperties properties;

//        // counters
//        private readonly LongAdder invokeCounter;
//        private readonly LongAdder successCounter;
//        private readonly LongAdder failureCounter;

//        private readonly HystrixIntegerCircularBuffer requestLatencyBuffer;

//        private string operationName;

//        private ConcurrentDictionary<string, ConcurrentDictionary<string, LongAdder>> exceptionCounter;

//        public InvocationMetrics(IInvocationMetricsProperties properties, string opName)
//        {
//            this.properties = properties;
//            this.invokeCounter = new LongAdder();
//            this.successCounter = new LongAdder();
//            this.failureCounter = new LongAdder();

//            int timeWindowInSeconds = properties.MetricsIntegerBufferTimeWindowInSeconds.Get();
//            int bucketTimeWindowInSeconds = properties.MetricsIntegerBufferBucketTimeWindowInSeconds.Get();
//            int bucketSizeLimit = properties.MetricsIntegerBufferBucketSizeLimit.Get();
//            this.requestLatencyBuffer = new HystrixIntegerCircularBuffer(timeWindowInSeconds, bucketTimeWindowInSeconds, bucketSizeLimit);

//            this.operationName = opName;
//            this.exceptionCounter = new ConcurrentDictionary<string, ConcurrentDictionary<string, LongAdder>>();
//        }

//        public long GetInvocationCount()
//        {
//            return this.invokeCounter.Sum();
//        }

//        public long GetSuccessCount()
//        {
//            return this.successCounter.Sum();
//        }

//        public long GetFailureCount()
//        {
//            return this.failureCounter.Sum();
//        }

//        public void GetInvocationTimeMetricsData(out int count, out long sum, out long min, out long max)
//        {
//            this.requestLatencyBuffer.GetAuditData(out count, out sum, out min, out max);
//        }

//        public long GetInvocationTimeAvg()
//        {
//            return this.requestLatencyBuffer.GetAuditDataAvg();
//        }

//        public int GetInvocationCountInTimeRange(long low, long? high = null)
//        {
//            return this.requestLatencyBuffer.GetItemCountInRange(low, high);
//        }

//        public ConcurrentDictionary<string, ConcurrentDictionary<string, LongAdder>> GetInvocationExceptionCounter()
//        {
//            return this.exceptionCounter;
//        }

//        public void MarkSuccess()
//        {
//            this.successCounter.Increment();
//        }

//        public void MarkFailure()
//        {
//            this.failureCounter.Increment();
//        }

//        public void MarkException(Exception ex)
//        {
//            string exceptionTypeName = "other";
//            if (ex is CServiceException)
//            {
//                var ex2 = ex as CServiceException;
//                if (ex2.ResponseErrors != null && ex2.ResponseErrors.Count > 0)
//                    exceptionTypeName = ex2.ResponseErrors[0].ErrorClassification.ToString().ToLower();
//            }
//            else if (ex is WebException)
//            {
//                var webException = ex as WebException;
//                if (webException is WebProtocolException)
//                {
//                    var protocalException = webException as WebProtocolException;
//                    switch (protocalException.StatusCode)
//                    {
//                        case (int)HttpStatusCode.RequestTimeout:
//                            exceptionTypeName = "timeout";
//                            break;
//                        default:
//                            exceptionTypeName = protocalException.StatusCode.ToString();
//                            break;
//                    }
//                }
//                else 
//                    exceptionTypeName = webException.Status.ToString().ToLower();
//            }

//            ConcurrentDictionary<string, ConcurrentDictionary<string, LongAdder>> currentExceptionCounter = exceptionCounter;
//            if (!currentExceptionCounter.ContainsKey(exceptionTypeName))
//                currentExceptionCounter.TryAdd(exceptionTypeName, new ConcurrentDictionary<string, LongAdder>());

//            string exceptionName = (ex.InnerException ?? ex).GetType().FullName;
//            if (!currentExceptionCounter[exceptionTypeName].ContainsKey(exceptionName))
//                currentExceptionCounter[exceptionTypeName].TryAdd(exceptionName, new LongAdder());

//            currentExceptionCounter[exceptionTypeName][exceptionName].Increment();
//        }

//        public void MarkInvocation()
//        {
//            this.invokeCounter.Increment();
//        }

//        public void ResetCounters()
//        {
//            this.invokeCounter.Reset();
//            this.successCounter.Reset();
//            this.failureCounter.Reset();
//            this.exceptionCounter = new ConcurrentDictionary<string, ConcurrentDictionary<string, LongAdder>>();
//        }

//        public void AddTotalInvocationTime(long duration)
//        {
//            this.requestLatencyBuffer.Add(duration);
//        }
//    }
//}
