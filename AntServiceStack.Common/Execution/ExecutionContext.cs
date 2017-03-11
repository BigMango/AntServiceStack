using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.Common.Utils;

namespace AntServiceStack.Common.Execution
{
    public class ExecutionContext
    {
        public string ServiceKey { get; internal set; }
        public string ServiceName { get; set; }
        public string ServiceNamespace { get; set; }
        public string ServiceContact { get; set; }
        public string ServiceUrl { get; set; }
        public string Format { get; set; }
        public string Operation { get; set; }
        public string OperationKey { get; internal set; }
        public string ExecutionMode { get; set; }

        public object Request { get; set; }
        public object Response { get; set; }

        public ExecutionMetrics Metrics { get; private set; }

        public bool IsSuccess { get; set; }

        public Exception Error { get; set; }

        public DateTime StartTime { get; internal set; }

        public ExecutionContext()
        {
            Metrics = new ExecutionMetrics();
            StartTime = DateTime.Now;
        }
    }
}
