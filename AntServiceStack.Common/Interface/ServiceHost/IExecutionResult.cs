using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.ServiceHost
{
    /// <summary>
    /// interface for value object holding service exection result,
    /// leveraged by AntServiceStack.Common.Hystrix for metrics and rate limiting
    /// </summary>
    public interface IExecutionResult
    {
        bool ValidationExceptionThrown { get; set; }

        bool FrameworkExceptionThrown { get; set; }

        bool ServiceExceptionThrown { get; set; }

        long ResponseSize { get; set; }

        long ServiceExecutionTime { get; set; }

        Exception ExceptionCaught { get; set; }
    }
}
