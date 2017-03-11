//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Freeway.Tracing;
//using System.Net;
//using AntServiceStack.Common.Utils;
//using AntServiceStack.Common.Trace;
//using Freeway.Gen.V2;

//namespace AntServiceStack.Client.Trace
//{
//    internal class IOCPRequestSpanWrapper : SpanWrapper
//    {
//        private SpanWrapper parentSpan;

//        public IOCPRequestSpanWrapper(SpanWrapper parentSpan, string serviceName)
//            : base(parentSpan.Tracer, InternalServiceUtils.IOCPRequestSpanName, serviceName, SpanType.WEB_SERVICE)
//        {
//            this.parentSpan = parentSpan;
//        }

//        public void PrepareRequest(HttpWebRequest httpWebRequest)
//        {
//            try
//            {
//                if (Span == null)
//                    return;

//                httpWebRequest.Headers[ServiceUtils.TRACE_ID_HTTP_HEADER] = Span.TraceId.ToString();
//                httpWebRequest.Headers[ServiceUtils.SPAN_ID_HTTP_HEADER] = Span.SpanId.ToString();
//            }
//            catch { }
//        }

//        public void Fork()
//        {
//            base.Fork(parentSpan);
//        }
//    }
//}
