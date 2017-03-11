//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Freeway.Tracing;
//using AntServiceStack.Common.Utils;
//using AntServiceStack.Common.Trace;
//using Freeway.Gen.V2;

//namespace AntServiceStack.Client.Trace
//{
//    internal class IOCPResponseSpanWrapper : SpanWrapper
//    {
//        private SpanWrapper parentSpan;

//        public IOCPResponseSpanWrapper(SpanWrapper parentSpan, string serviceName)
//            : base(parentSpan.Tracer, InternalServiceUtils.IOCPResponseSpanName, serviceName, SpanType.WEB_SERVICE)
//        {
//            this.parentSpan = parentSpan;
//        }

//        public void Fork()
//        {
//            base.Fork(parentSpan);
//        }
//    }
//}
