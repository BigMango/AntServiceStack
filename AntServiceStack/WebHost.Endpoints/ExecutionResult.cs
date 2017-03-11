using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntServiceStack.ServiceHost;

namespace AntServiceStack.WebHost.Endpoints
{
    /// <summary>
    /// Value object holding service exection result,
    /// leveraged by AntServiceStack.Common.Hystrix for metrics and rate limiting
    /// </summary>
    public class ExecutionResult : IExecutionResult
    {
        public bool ValidationExceptionThrown { get; set; }

        public bool FrameworkExceptionThrown { get; set; }

        public bool ServiceExceptionThrown { get; set; }

        public long ResponseSize { get; set; }

        public long ServiceExecutionTime { get; set; }

        public Exception ExceptionCaught { get; set; }
    }
}
