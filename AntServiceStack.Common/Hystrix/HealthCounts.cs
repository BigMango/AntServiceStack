using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntServiceStack.Common.Hystrix
{
    /// <summary>
    /// Stores summarized health metrics about HystrixCommands.
    /// </summary>
    public class HealthCounts
    {
        public long TotalRequests { get; set; }

        public long TotalErrorCount { get; set; }

        public long TotalExceptionCount { get; set; }

        public long TotalFailureCount { get; set; }

        public long TotalSuccessCount { get; set; }

        public int ErrorPercentage { get; set; }

        public long SuccessCount { get; private set; }

        public long TimeoutCount { get; private set; }

        public long ThreadPoolRejectedCount { get; private set; }

        public long ShortCircuitedCount { get; private set; }

        public long FrameworkExceptionCount { get; private set; }

        public long ServiceExceptionCount { get; private set; }

        public long ValidationExceptionCount { get; private set; }

        public HealthCounts()
        {
        }

        /// <summary>
        /// Initializes a new instance of HealthCounts.
        /// </summary>
        /// <param name="total">The total number of requests made by this command.</param>
        /// <param name="error">The total number of errors made by this command.</param>
        public HealthCounts(long successCount, long timeoutCount, long threadPoolRejectedCount, long shortCircuitedCount,
            long frameworkExceptionCount, long serviceExceptionCount, long validationExceptionCount)
        {
            SuccessCount = successCount;
            TimeoutCount = timeoutCount;
            ThreadPoolRejectedCount = threadPoolRejectedCount;
            ShortCircuitedCount = shortCircuitedCount;
            FrameworkExceptionCount = frameworkExceptionCount;
            ServiceExceptionCount = serviceExceptionCount;
            ValidationExceptionCount = validationExceptionCount;

            TotalRequests = SuccessCount + TimeoutCount + ThreadPoolRejectedCount + ShortCircuitedCount + FrameworkExceptionCount
                + ServiceExceptionCount + ValidationExceptionCount;
            TotalErrorCount = TimeoutCount + ThreadPoolRejectedCount + ShortCircuitedCount + FrameworkExceptionCount + ServiceExceptionCount;
            TotalExceptionCount = ThreadPoolRejectedCount + ShortCircuitedCount + FrameworkExceptionCount
                + ServiceExceptionCount + ValidationExceptionCount;
            TotalFailureCount = FrameworkExceptionCount + ServiceExceptionCount;

            if (TotalRequests > 0)
                ErrorPercentage = (int)((double)TotalErrorCount / TotalRequests * 100);
        }
    }
}
