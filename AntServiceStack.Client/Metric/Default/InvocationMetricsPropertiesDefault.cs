//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using AntServiceStack.Common.Hystrix;

//namespace AntServiceStack.Client.Metric.Default
//{
//    public class InvocationMetricsPropertiesDefault : IInvocationMetricsProperties
//    {
//        private const int DefaultMetricsIntegerBufferTimeWindowInSeconds = 60;
//        private const int DefaultMetricsIntegerBufferBucketTimeWindowInSeconds = 10;
//        private const int DefaultMetricsIntegerBufferBucketSizeLimit = 200;

//        public IHystrixProperty<int> MetricsIntegerBufferTimeWindowInSeconds
//        {
//            get { return HystrixPropertyFactory.AsProperty(DefaultMetricsIntegerBufferTimeWindowInSeconds); }
//        }

//        public IHystrixProperty<int> MetricsIntegerBufferBucketTimeWindowInSeconds
//        {
//            get { return HystrixPropertyFactory.AsProperty(DefaultMetricsIntegerBufferBucketTimeWindowInSeconds); }
//        }

//        public IHystrixProperty<int> MetricsIntegerBufferBucketSizeLimit
//        {
//            get { return HystrixPropertyFactory.AsProperty(DefaultMetricsIntegerBufferBucketSizeLimit); }
//        }
//    }
//}
