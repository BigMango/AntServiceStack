//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace AntServiceStack.Common.Trace
//{
//    internal class SpanWrapper
//    {
//        public ITrace Tracer { get; private set; }

//        public ISpan Span { get; private set; }

//        public string SpanName { get; private set; }

//        public string ServiceName { get; private set; }

//        public SpanType SpanType { get; private set; }

//        public long? SpanId 
//        { 
//            get 
//            {
//                if (Span == null)
//                    return null;
//                return Span.SpanId; 
//            } 
//        }

//        public long? TraceId
//        {
//            get
//            {
//                if (Span == null)
//                    return null;
//                return Span.TraceId;
//            }
//        }

//        public SpanWrapper(ITrace tracer, string spanName, string serviceName, SpanType spanType)
//        {
//            this.Tracer = tracer;
//            this.SpanName = spanName;
//            this.ServiceName = serviceName;
//            this.SpanType = spanType;
//        }

//        public virtual void Start()
//        {
//            try
//            {
//                if (Tracer.IsTracing)
//                    Span = Tracer.StartSpan(SpanName, ServiceName, SpanType);
//            }
//            catch { }
//        }

//        public virtual void Fork(SpanWrapper parentSpan)
//        {
//            try
//            {
//                if (parentSpan != null && parentSpan.TraceId.HasValue && parentSpan.SpanId.HasValue)
//                    Span = Tracer.ContinueSpan(SpanName, ServiceName, parentSpan.TraceId.Value, parentSpan.SpanId.Value, SpanType);
//            }
//            catch { }
//        }

//        public virtual void Stop()
//        {
//            try
//            {
//                if (Span == null)
//                    return;
                
//                Span.Stop();
//            }
//            catch { }
//        }
//    }
//}
